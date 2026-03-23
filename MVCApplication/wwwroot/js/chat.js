/** Same-origin chat API (MVC → ChatApiController → APIGateway → ChatbotAPI). */
var CHAT_MESSAGE_URL = '/api/chat/message';
var CHAT_HEALTH_URL = '/api/chat/health';

/** Escape HTML then show newlines; strip Markdown-style ** so text looks clean in chat. */
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function formatBotMessageText(raw) {
    if (raw == null || raw === '') return '';
    var s = String(raw).replace(/\r\n/g, '\n').replace(/\\n/g, '\n');
    s = s.split('**').join('');
    s = s.split('__').join('');
    s = s.replace(/(\S)\s*\*\s+(?=\S)/g, function (_, last) {
        return last + '\n• ';
    });
    s = s.replace(/^\*\s+/gm, '• ');
    s = s.replace(/\n{3,}/g, '\n\n');
    s = s.trim();
    return escapeHtml(s).replace(/\n/g, '<br>');
}

function unwrapApiResponse(resp) {
    if (!resp) return { success: false, data: null };
    var ok = resp.success === true || resp.Success === true;
    var data = resp.data !== undefined ? resp.data : resp.Data;
    return { success: ok, data: data };
}

var chatbox = {
    isOpen: false,
    sessionId: null,
    messages: [],
    unreadCount: 0,         // badge count when widget is closed
    pendingRetry: null,     // { message, xhr } for retry on error

    /** ─── localStorage persistence ─────────────────────────────────────────── */
    saveState: function() {
        try {
            localStorage.setItem('chatbox_sessionId', this.sessionId || '');
            localStorage.setItem('chatbox_messages', JSON.stringify(this.messages));
        } catch (e) {}
    },
    loadState: function() {
        try {
            var currentCustomerId = $('#customer-id').val() || '';
            var storedCustomerId = localStorage.getItem('chatbox_customerId') || '';

            if (currentCustomerId !== storedCustomerId) {
                localStorage.removeItem('chatbox_sessionId');
                localStorage.removeItem('chatbox_messages');
                localStorage.removeItem('chatbox_customerId');
                this.sessionId = null;
                this.messages = [];
                return;
            }

            var sid = localStorage.getItem('chatbox_sessionId');
            this.sessionId = sid && sid.length > 0 ? sid : null;
            var raw = localStorage.getItem('chatbox_messages');
            if (raw) {
                try { this.messages = JSON.parse(raw); } catch (e) { this.messages = []; }
            }
        } catch (e) {
            this.sessionId = null;
            this.messages = [];
        }
    },

    /** ─── Init ────────────────────────────────────────────────────────────── */
    init: function() {
        var self = this;
        this.loadState();
        this.setupEvents();
        this.updateHealthStatus();
        setInterval(function() { self.updateHealthStatus(); }, 30000); // poll health every 30s
        if (this.messages.length > 0) {
            this.renderAllMessages();
        }
    },
    setupEvents: function() {
        var self = this;

        $(document).on('click', '#chat-toggle-btn', function() {
            self.toggle();
        });

        $(document).on('click', '#chat-send-btn', function() {
            self.sendMessage();
        });

        $(document).on('keypress', '#chat-input', function(e) {
            if (e.which === 13 && !e.shiftKey) {
                e.preventDefault();
                self.sendMessage();
            }
        });

        $(document).on('click', '.chat-quick-btn', function() {
            var msg = $(this).data('msg');
            if (msg) {
                $('#chat-input').val(msg);
                self.sendMessage();
            }
        });

        $(document).on('click', '#chat-clear-btn', function() {
            self.clearChat();
        });

        $(document).on('click', '.chat-suggested-action', function() {
            var msg = $(this).data('action');
            if (msg) {
                $('#chat-input').val(msg);
                self.sendMessage();
            }
        });

        $(document).on('click', '#chat-retry-btn', function() {
            self.retryLastFailed();
        });
    },

    /** ─── Toggle widget ───────────────────────────────────────────────────── */
    toggle: function() {
        this.isOpen = !this.isOpen;
        var widget = $('#chat-widget');
        var container = $('#chat-container');

        if (this.isOpen) {
            this.unreadCount = 0;
            this.updateBadge();

            widget.addClass('open');
            container.addClass('chat-open').hide().slideDown(300);

            if (!this.sessionId) {
                this.initChat();
            }
        } else {
            container.slideUp(300, function() {
                container.removeClass('chat-open');
                widget.removeClass('open');
            });
        }
    },

    /** ─── Health check — updates the Online dot ──────────────────────────── */
    updateHealthStatus: function() {
        var self = this;
        $.ajax({
            url: CHAT_HEALTH_URL,
            method: 'GET',
            timeout: 5000,
            success: function() {
                self.setHealthDot(true);
            },
            error: function() {
                self.setHealthDot(false);
            }
        });
    },
    setHealthDot: function(online) {
        var dot = $('#chat-header-dot');
        if (online) {
            dot.removeClass('offline').addClass('online');
            dot.attr('title', 'Đang hoạt động');
        } else {
            dot.removeClass('online').addClass('offline');
            dot.attr('title', 'Đang bảo trì');
        }
    },

    /** ─── Notification badge ─────────────────────────────────────────────── */
    updateBadge: function() {
        var badge = $('#chat-unread-badge');
        if (this.unreadCount > 0 && !this.isOpen) {
            badge.text(this.unreadCount > 9 ? '9+' : this.unreadCount).show();
        } else {
            badge.hide();
        }
    },

    /** ─── Clear chat ─────────────────────────────────────────────────────── */
    clearChat: function() {
        this.messages = [];
        this.sessionId = null;
        this.pendingRetry = null;
        localStorage.removeItem('chatbox_sessionId');
        localStorage.removeItem('chatbox_messages');
        $('#chat-messages').empty();
        $('#chat-suggested-actions').empty().hide();
        $('#chat-init').hide();
        this.initChat();
    },

    /** ─── Init greeting ──────────────────────────────────────────────────── */
    initChat: function() {
        var self = this;
        var customerId = $('#customer-id').val();

        if (this.sessionId) {
            this.renderAllMessages();
            return;
        }

        $('#chat-init').hide();
        this.showTyping();

        $.ajax({
            url: CHAT_MESSAGE_URL,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                message: "Xin chào",
                customerId: customerId,
                customerName: $('#customer-name').val()
            }),
            success: function(response) {
                self.hideTyping();
                var init = unwrapApiResponse(response);
                if (init.success && init.data) {
                    self.sessionId = init.data.sessionId || init.data.SessionId;
                    self.saveState();
                    var text = init.data.response || init.data.Response || '';
                    if (text) self.addMessage(text, 'bot');
                }
            },
            error: function(xhr) {
                self.hideTyping();
                self.showErrorBotMessage(self.getAjaxErrorMessage(xhr, 'init'));
            }
        });
    },

    /** ─── Send message ──────────────────────────────────────────────────── */
    sendMessage: function() {
        var input = $('#chat-input');
        var message = input.val().trim();
        if (!message) return;

        try { localStorage.setItem('chatbox_customerId', $('#customer-id').val() || ''); } catch (e) {}

        if (!this.sessionId) {
            this.sessionId = this.generateSessionId();
            this.saveState();
        }

        this.addMessage(message, 'user');
        input.val('');
        this.showTyping();
        $('#chat-suggested-actions').empty().hide();
        $('#chat-init').hide();
        $('#chat-retry-row').remove();

        var self = this;
        var customerId = $('#customer-id').val();
        var customerName = $('#customer-name').val();

        $.ajax({
            url: CHAT_MESSAGE_URL,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                message: message,
                sessionId: this.sessionId,
                customerId: customerId,
                customerName: customerName
            }),
            success: function(response) {
                self.hideTyping();
                self.pendingRetry = null;
                var r = unwrapApiResponse(response);
                if (r.success && r.data) {
                    var reply = r.data.response || r.data.Response || '';
                    if (reply) {
                        self.addMessage(reply, 'bot');
                        self.showSuggestedActions(r.data.suggestedActions || r.data.SuggestedActions);
                    } else {
                        self.addMessage('Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.', 'bot');
                    }
                } else {
                    self.addMessage('Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.', 'bot');
                }
            },
            error: function(xhr) {
                self.hideTyping();
                self.pendingRetry = { message: message };
                var msg = self.getAjaxErrorMessage(xhr, 'send');
                self.addMessage(msg, 'bot');
                self.showRetryRow();
            }
        });
    },

    /** ─── Retry failed message ───────────────────────────────────────────── */
    retryLastFailed: function() {
        $('#chat-retry-row').remove();
        this.sendMessage();
    },

    /** ─── Suggested actions ──────────────────────────────────────────────── */
    showSuggestedActions: function(actions) {
        var container = $('#chat-suggested-actions');
        container.empty();

        if (!actions || !Array.isArray(actions) || actions.length === 0) {
            container.hide();
            return;
        }

        var html = '<div class="suggested-actions-label">Gợi ý:</div>';
        actions.forEach(function(action) {
            html += '<button type="button" class="chat-suggested-action" data-action="' + escapeHtml(action) + '">' + escapeHtml(action) + '</button>';
        });
        container.html(html).show();
    },

    /** ─── Retry row ──────────────────────────────────────────────────────── */
    showRetryRow: function() {
        $('#chat-retry-row').remove();
        var row = $('<div>').attr('id', 'chat-retry-row').addClass('chat-retry-row');
        row.html(
            '<span class="chat-retry-text">Không gửi được</span>' +
            '<button id="chat-retry-btn" class="chat-retry-btn"><i class="fas fa-redo"></i> Thử lại</button>'
        );
        $('#chat-messages').append(row);
        this.scrollToBottom();
    },

    /** ─── Error helpers ─────────────────────────────────────────────────── */
    getAjaxErrorMessage: function(xhr, context) {
        if (xhr && xhr.status === 429) {
            return 'Hệ thống AI đang quá tải (hết quota). Vui lòng thử lại sau vài phút.';
        }
        if (xhr && xhr.status === 403) {
            try {
                var b = JSON.parse(xhr.responseText || '{}');
                if (b && b.message) return b.message;
            } catch (e) {}
            return 'Google đã khóa API key hoặc dự án Gemini. Cần key mới tại Google AI Studio.';
        }
        if (xhr && xhr.status === 401) {
            return 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại để tiếp tục sử dụng chatbot.';
        }
        if (xhr && xhr.status === 0) {
            return 'Không thể kết nối chatbot. Vui lòng kiểm tra kết nối mạng và thử lại.';
        }
        return 'Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này. Vui lòng thử lại sau.';
    },
    showErrorBotMessage: function(text) {
        this.addMessage(text, 'bot');
    },

    /** ─── Message rendering ─────────────────────────────────────────────── */
    addMessage: function(text, sender) {
        this.messages.push({ text: text, sender: sender });

        var messageDiv = $('<div>').addClass('chat-message ' + sender);
        var contentDiv = $('<div>').addClass('message-content');
        if (sender === 'bot') {
            contentDiv.addClass('message-content-plain').html(formatBotMessageText(text));
        } else {
            contentDiv.text(text);
        }
        messageDiv.append(contentDiv);
        $('#chat-messages').append(messageDiv);
        this.scrollToBottom();
        this.saveState();

        if (sender === 'bot' && !this.isOpen) {
            this.unreadCount++;
            this.updateBadge();
        }
    },
    renderAllMessages: function() {
        var self = this;
        $('#chat-messages').empty();
        this.messages.forEach(function(msg) {
            var messageDiv = $('<div>').addClass('chat-message ' + msg.sender);
            var contentDiv = $('<div>').addClass('message-content');
            if (msg.sender === 'bot') {
                contentDiv.addClass('message-content-plain').html(formatBotMessageText(msg.text));
            } else {
                contentDiv.text(msg.text);
            }
            messageDiv.append(contentDiv);
            $('#chat-messages').append(messageDiv);
        });
        this.scrollToBottom();
    },
    showTyping: function() {
        var typingDiv = $('<div>').attr('id', 'chat-typing').addClass('chat-message bot')
            .html('<div class="message-content"><span class="typing-indicator"><i class="fas fa-ellipsis-h"></i></span></div>');
        $('#chat-messages').append(typingDiv);
        this.scrollToBottom();
    },
    hideTyping: function() {
        $('#chat-typing').remove();
    },
    scrollToBottom: function() {
        var el = $('#chat-messages');
        if (el.length) el.scrollTop(el[0].scrollHeight);
    },
    generateSessionId: function() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
};

$(document).ready(function() {
    chatbox.init();
});
