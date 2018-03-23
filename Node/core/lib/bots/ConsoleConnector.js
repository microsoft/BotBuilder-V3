"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
const utils = require("../utils");
const readline = require("readline");
const async = require("async");
class ConsoleConnector {
    constructor() {
        this.replyCnt = 0;
    }
    listen() {
        this.rl = readline.createInterface({ input: process.stdin, output: process.stdout, terminal: false });
        this.rl.on('line', (line) => {
            this.replyCnt = 0;
            line = line || '';
            if (line.toLowerCase() == 'quit') {
                this.rl.close();
                process.exit();
            }
            else {
                this.processMessage(line);
            }
        });
        return this;
    }
    processMessage(line) {
        if (this.onEventHandler) {
            let msg = new Message_1.Message()
                .address({
                channelId: 'console',
                user: { id: 'user', name: 'User1' },
                bot: { id: 'bot', name: 'Bot' },
                conversation: { id: 'Convo1' }
            })
                .timestamp()
                .text(line);
            this.onEventHandler([msg.toMessage()]);
        }
        return this;
    }
    processEvent(event) {
        if (this.onEventHandler) {
            this.onEventHandler([event]);
        }
        return this;
    }
    onEvent(handler) {
        this.onEventHandler = handler;
    }
    onInvoke(handler) {
        this.onInvokeHandler = handler;
    }
    send(messages, done) {
        let addresses = [];
        async.forEachOfSeries(messages, (msg, idx, cb) => {
            try {
                if (msg.type == 'delay') {
                    setTimeout(cb, msg.value);
                }
                else if (msg.type == 'message') {
                    if (this.replyCnt++ > 0) {
                        console.log();
                    }
                    if (msg.text) {
                        log(msg.text);
                    }
                    if (msg.attachments && msg.attachments.length > 0) {
                        for (let j = 0; j < msg.attachments.length; j++) {
                            if (j > 0) {
                                console.log();
                            }
                            renderAttachment(msg.attachments[j]);
                        }
                    }
                    let adr = utils.clone(msg.address);
                    adr.id = idx.toString();
                    addresses.push(adr);
                    cb(null);
                }
                else {
                    cb(null);
                }
            }
            catch (e) {
                cb(e);
            }
        }, (err) => done(err, !err ? addresses : null));
    }
    startConversation(address, cb) {
        let adr = utils.clone(address);
        adr.conversation = { id: 'Convo1' };
        cb(null, adr);
    }
}
exports.ConsoleConnector = ConsoleConnector;
function renderAttachment(a) {
    switch (a.contentType) {
        case 'application/vnd.microsoft.card.hero':
        case 'application/vnd.microsoft.card.thumbnail':
            let tc = a.content;
            if (tc.title) {
                if (tc.title.length <= 40) {
                    line('=', 60, tc.title);
                }
                else {
                    line('=', 60);
                    wrap(tc.title, 60, 3);
                }
            }
            if (tc.subtitle) {
                wrap(tc.subtitle, 60, 3);
            }
            if (tc.text) {
                wrap(tc.text, 60, 3);
            }
            renderImages(tc.images);
            renderButtons(tc.buttons);
            break;
        case 'application/vnd.microsoft.card.signin':
        case 'application/vnd.microsoft.card.receipt':
        default:
            line('.', 60, a.contentType);
            if (a.contentUrl) {
                wrap(a.contentUrl, 60, 3);
            }
            else {
                log(JSON.stringify(a.content));
            }
            break;
    }
}
function renderImages(images) {
    if (images && images.length) {
        line('.', 60, 'images');
        let bullet = images.length > 1 ? '* ' : '';
        for (let i = 0; i < images.length; i++) {
            let img = images[i];
            if (img.alt) {
                wrap(bullet + img.alt + ': ' + img.url, 60, 3);
            }
            else {
                wrap(bullet + img.url, 60, 3);
            }
        }
    }
}
function renderButtons(actions) {
    if (actions && actions.length) {
        line('.', 60, 'buttons');
        let bullet = actions.length > 1 ? '* ' : '';
        for (let i = 0; i < actions.length; i++) {
            let a = actions[i];
            if (a.title == a.value) {
                wrap(bullet + a.title, 60, 3);
            }
            else {
                wrap(bullet + a.title + ' [' + a.value + ']', 60, 3);
            }
        }
    }
}
function line(char, length, title) {
    if (title) {
        let txt = repeat(char, 2);
        txt += '[' + title + ']';
        if (length > txt.length) {
            txt += repeat(char, length - txt.length);
        }
        log(txt);
    }
    else {
        log(repeat(char, length));
    }
}
function wrap(text, length, indent = 0) {
    let buffer = '';
    let pad = indent ? repeat(' ', indent) : '';
    let tokens = text.split(' ');
    length -= pad.length;
    for (let i = 0; i < tokens.length; i++) {
        let t = tokens[i];
        if (buffer.length) {
            if ((buffer.length + 1 + t.length) > length) {
                log(pad + buffer);
                buffer = t;
            }
            else {
                buffer += ' ' + t;
            }
        }
        else if (t.length < length) {
            buffer = t;
        }
        else {
            log(pad + t);
        }
    }
    if (buffer.length) {
        log(pad + buffer);
    }
}
function repeat(char, length) {
    let txt = '';
    for (let i = 0; i < length; i++) {
        txt += char;
    }
    return txt;
}
function log(text) {
    console.log(text);
}
