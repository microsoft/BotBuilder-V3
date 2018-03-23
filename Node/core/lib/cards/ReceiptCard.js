"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
class ReceiptCard {
    constructor(session) {
        this.session = session;
        this.data = {
            contentType: 'application/vnd.microsoft.card.receipt',
            content: {}
        };
    }
    title(text, ...args) {
        if (text) {
            this.data.content.title = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    items(list) {
        this.data.content.items = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var item = list[i];
                this.data.content.items.push(item.toItem ? item.toItem() : item);
            }
        }
        return this;
    }
    facts(list) {
        this.data.content.facts = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var fact = list[i];
                this.data.content.facts.push(fact.toFact ? fact.toFact() : fact);
            }
        }
        return this;
    }
    tap(action) {
        if (action) {
            this.data.content.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    }
    total(v) {
        this.data.content.total = v || '';
        return this;
    }
    tax(v) {
        this.data.content.tax = v || '';
        return this;
    }
    vat(v) {
        this.data.content.vat = v || '';
        return this;
    }
    buttons(list) {
        this.data.content.buttons = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var action = list[i];
                this.data.content.buttons.push(action.toAction ? action.toAction() : action);
            }
        }
        return this;
    }
    toAttachment() {
        return this.data;
    }
}
exports.ReceiptCard = ReceiptCard;
class ReceiptItem {
    constructor(session) {
        this.session = session;
        this.data = {};
    }
    title(text, ...args) {
        if (text) {
            this.data.title = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    subtitle(text, ...args) {
        if (text) {
            this.data.subtitle = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    text(text, ...args) {
        if (text) {
            this.data.text = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    image(img) {
        if (img) {
            this.data.image = img.toImage ? img.toImage() : img;
        }
        return this;
    }
    price(v) {
        this.data.price = v || '';
        return this;
    }
    quantity(v) {
        this.data.quantity = v || '';
        return this;
    }
    tap(action) {
        if (action) {
            this.data.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    }
    toItem() {
        return this.data;
    }
    static create(session, price, title) {
        return new ReceiptItem(session).price(price).title(title);
    }
}
exports.ReceiptItem = ReceiptItem;
class Fact {
    constructor(session) {
        this.session = session;
        this.data = { value: '' };
    }
    key(text, ...args) {
        if (text) {
            this.data.key = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    value(v) {
        this.data.value = v || '';
        return this;
    }
    toFact() {
        return this.data;
    }
    static create(session, value, key) {
        return new Fact(session).value(value).key(key);
    }
}
exports.Fact = Fact;
