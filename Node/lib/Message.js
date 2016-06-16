var sprintf = require('sprintf-js');
var utils = require('./utils');
var hc = require('./cards/HeroCard');
var img = require('./cards/CardImage');
var ca = require('./cards/CardAction');
exports.LayoutStyle = {
    auto: null,
    image: 'image',
    moji: 'moji',
    card: 'card',
    signinCard: 'card.signin',
    receiptCard: 'card.receipt',
    carouselCard: 'card.carousel'
};
var Message = (function () {
    function Message(session) {
        this.session = session;
        this.data = {};
        this.style = exports.LayoutStyle.auto;
        this.data.type = 'message';
        if (this.session) {
            var m = this.session.message;
            if (m.local) {
                this.data.local = m.local;
            }
            if (m.address) {
                this.data.address = m.address;
            }
        }
    }
    Message.prototype.layoutStyle = function (style) {
        this.style = style;
        return this;
    };
    Message.prototype.local = function (local) {
        this.data.local = local;
        return this;
    };
    Message.prototype.text = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        this.data.text = text ? fmtText(this.session, text, args) : '';
        return this;
    };
    Message.prototype.ntext = function (msg, msg_plural, count) {
        var fmt = count == 1 ? Message.randomPrompt(msg) : Message.randomPrompt(msg_plural);
        if (this.session) {
            fmt = this.session.gettext(fmt);
        }
        this.data.text = sprintf.sprintf(fmt, count);
        return this;
    };
    Message.prototype.compose = function (prompts) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (prompts) {
            this.data.text = Message.composePrompt(this.session, prompts, args);
        }
        return this;
    };
    Message.prototype.summary = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        this.data.summary = text ? fmtText(this.session, text, args) : '';
        return this;
    };
    Message.prototype.attachments = function (list) {
        this.data.attachments = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                this.addAttachment(list[i]);
            }
        }
        return this;
    };
    Message.prototype.addAttachment = function (attachment) {
        if (attachment) {
            var a = attachment.toAttachment ? attachment.toAttachment() : attachment;
            a = this.upgradeAttachment(a);
            if (!this.data.attachments) {
                this.data.attachments = [a];
            }
            else {
                this.data.attachments.push(a);
            }
        }
        return this;
    };
    Message.prototype.upgradeAttachment = function (a) {
        var isOldSchema = false;
        for (var prop in a) {
            switch (prop) {
                case 'actions':
                case 'fallbackText':
                case 'title':
                case 'titleLink':
                case 'text':
                case 'thumbnailUrl':
                    isOldSchema = true;
                    break;
            }
        }
        if (isOldSchema) {
            console.warn('Using old attachment schema. Upgrade to new card schema.');
            var v2 = a;
            var card = new hc.HeroCard();
            if (v2.title) {
                card.title(v2.title);
            }
            if (v2.text) {
                card.text(v2.text);
            }
            if (v2.thumbnailUrl) {
                card.images([new img.CardImage().url(v2.thumbnailUrl)]);
            }
            if (v2.titleLink) {
                card.tap(ca.CardAction.openUrl(null, v2.titleLink));
            }
            if (v2.actions) {
                var list = [];
                for (var i = 0; i < v2.actions.length; i++) {
                    var old = v2.actions[i];
                    var btn = old.message ?
                        ca.CardAction.postBack(null, old.message, old.title) :
                        ca.CardAction.openUrl(null, old.url, old.title);
                    if (old.image) {
                        btn.image(old.image);
                    }
                    list.push(btn);
                }
                card.buttons(list);
            }
            return card.toAttachment();
        }
        else {
            return a;
        }
    };
    Message.prototype.entities = function (list) {
        this.data.entities = list || [];
        return this;
    };
    Message.prototype.addEntity = function (obj) {
        if (obj) {
            if (!this.data.entities) {
                this.data.entities = [obj];
            }
            else {
                this.data.entities.push(obj);
            }
        }
        return this;
    };
    Message.prototype.address = function (adr) {
        if (adr) {
            this.data.address = adr;
        }
        return this;
    };
    Message.prototype.timestamp = function (time) {
        this.data.timestamp = time || new Date().toISOString();
        return this;
    };
    Message.prototype.channelData = function (map) {
        if (map) {
            var channelId = this.data.address ? this.data.address.channelId : '*';
            if (map.hasOwnProperty(channelId)) {
                this.data.channelData = map[channelId];
            }
            else if (map.hasOwnProperty('*')) {
                this.data.channelData = map['*'];
            }
        }
        return this;
    };
    Message.prototype.toMessage = function () {
        var style = this.style;
        if (!style && this.data.attachments) {
            var cards = 0;
            var hasReceipt = false;
            var hasSignin = false;
            var attachments = this.data.attachments;
            for (var i = 0; i < attachments.length; i++) {
                var ct = attachments[i].contentType || '';
                if (ct.indexOf('vnd.microsoft.card') > 0) {
                    cards++;
                    if (ct.indexOf('card.signin') > 0) {
                        hasSignin = true;
                    }
                    else if (ct.indexOf('card.receipt') > 0) {
                        hasReceipt = true;
                    }
                }
            }
            if (cards > 0) {
                if (cards > 1) {
                    style = exports.LayoutStyle.carouselCard;
                }
                else if (hasSignin) {
                    style = exports.LayoutStyle.signinCard;
                }
                else if (hasReceipt) {
                    style = exports.LayoutStyle.receiptCard;
                }
                else {
                    style = exports.LayoutStyle.card;
                }
            }
            else if (attachments.length > 0) {
                var ct = attachments[0].contentType || '';
                if (ct.indexOf('image') == 0 || ct.indexOf('vnd.microsoft.image') > 0) {
                    style = exports.LayoutStyle.image;
                }
                else if (ct.indexOf('vnd.microsoft.moji') > 0) {
                    style = exports.LayoutStyle.moji;
                }
            }
        }
        var msg = utils.clone(this.data);
        if (style) {
            msg.type += '/' + style;
        }
        return msg;
    };
    Message.randomPrompt = function (prompts) {
        if (Array.isArray(prompts)) {
            var i = Math.floor(Math.random() * prompts.length);
            return prompts[i];
        }
        else {
            return prompts;
        }
    };
    Message.composePrompt = function (session, prompts, args) {
        var connector = '';
        var prompt = '';
        for (var i = 0; i < prompts.length; i++) {
            var txt = Message.randomPrompt(prompts[1]);
            prompt += connector + (session ? session.gettext(txt) : txt);
            connector = ' ';
        }
        return args && args.length > 0 ? sprintf.vsprintf(prompt, args) : prompt;
    };
    Message.prototype.setLanguage = function (local) {
        console.warn("Message.setLanguage() is deprecated. Use Message.local() instead.");
        return this.local(local);
    };
    Message.prototype.setText = function (session, prompts) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        console.warn("Message.setText() is deprecated. Use Message.text() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        args.unshift(prompts);
        return Message.prototype.text.apply(this, args);
    };
    Message.prototype.setNText = function (session, msg, msg_plural, count) {
        console.warn("Message.setNText() is deprecated. Use Message.ntext() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        return this.ntext(msg, msg_plural, count);
    };
    Message.prototype.composePrompt = function (session, prompts) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        console.warn("Message.composePrompt() is deprecated. Use Message.compose() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        args.unshift(prompts);
        return Message.prototype.compose.apply(this, args);
    };
    Message.prototype.setChannelData = function (data) {
        console.warn("Message.setChannelData() is deprecated. Use Message.channelData() instead.");
        return this.channelData({ '*': data });
    };
    return Message;
})();
exports.Message = Message;
function fmtText(session, prompts, args) {
    var fmt = Message.randomPrompt(prompts);
    if (session) {
        fmt = session.gettext(fmt);
    }
    return args && args.length > 0 ? sprintf.vsprintf(fmt, args) : fmt;
}
exports.fmtText = fmtText;
