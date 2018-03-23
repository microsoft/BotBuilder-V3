"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const SessionLogger_1 = require("./SessionLogger");
class RemoteSessionLogger extends SessionLogger_1.SessionLogger {
    constructor(connector, address, relatesTo) {
        super();
        this.connector = connector;
        this.address = address;
        this.relatesTo = relatesTo;
        this.isEnabled = true;
        this.event = this.createEvent();
    }
    dump(name, value) {
        super.dump(name, value);
        this.event.value.push({
            type: 'variable',
            timestamp: new Date().getTime(),
            name: name,
            value: value
        });
    }
    log(dialogStack, msg, ...args) {
        super.log.apply(this, [dialogStack, msg].concat(args));
        this.event.value.push({
            type: 'log',
            timestamp: new Date().getTime(),
            level: 'info',
            msg: msg,
            args: args
        });
    }
    warn(dialogStack, msg, ...args) {
        super.warn.apply(this, [dialogStack, msg].concat(args));
        this.event.value.push({
            type: 'log',
            timestamp: new Date().getTime(),
            level: 'warn',
            msg: msg,
            args: args
        });
    }
    error(dialogStack, err) {
        super.error(dialogStack, err);
        this.event.value.push({
            type: 'log',
            timestamp: new Date().getTime(),
            level: 'info',
            msg: err.stack
        });
    }
    flush(callback) {
        var ev = this.event;
        this.event = this.createEvent();
        this.connector.send([ev], callback);
    }
    createEvent() {
        return {
            type: 'event',
            address: this.address,
            name: 'debug',
            value: [],
            relatesTo: this.relatesTo,
            text: "Debug Event"
        };
    }
}
exports.RemoteSessionLogger = RemoteSessionLogger;
