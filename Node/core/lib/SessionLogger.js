"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const logger = require("./logger");
class SessionLogger {
    constructor() {
        this.isEnabled = new RegExp('\\bbotbuilder\\b', 'i').test(process.env.NODE_DEBUG || '');
    }
    dump(name, value) {
        if (this.isEnabled && name) {
            if (Array.isArray(value) || typeof value == 'object') {
                try {
                    var v = JSON.stringify(value);
                    console.log(name + ': ' + v);
                }
                catch (e) {
                    console.error(name + ': {STRINGIFY ERROR}');
                }
            }
            else {
                console.log(name + ': ' + value);
            }
        }
    }
    log(dialogStack, msg, ...args) {
        if (this.isEnabled && msg) {
            var prefix = logger.getPrefix(dialogStack);
            args.unshift(prefix + msg);
            console.log.apply(console, args);
        }
    }
    warn(dialogStack, msg, ...args) {
        if (this.isEnabled && msg) {
            var prefix = logger.getPrefix(dialogStack);
            args.unshift(prefix + 'WARN: ' + msg);
            console.warn.apply(console, args);
        }
    }
    error(dialogStack, err) {
        if (this.isEnabled && err) {
            var prefix = logger.getPrefix(dialogStack);
            console.error(prefix + 'ERROR: ' + err.message);
        }
    }
    flush(callback) {
        callback(null);
    }
}
exports.SessionLogger = SessionLogger;
