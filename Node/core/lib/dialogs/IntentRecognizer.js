"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class IntentRecognizer {
    constructor() {
        this._onEnabled = [];
        this._onFilter = [];
    }
    recognize(context, callback) {
        this.isEnabled(context, (err, enabled) => {
            if (err) {
                callback(err, null);
            }
            else if (!enabled) {
                callback(null, { score: 0.0, intent: null });
            }
            else {
                this.onRecognize(context, (err, result) => {
                    if (!err) {
                        this.filter(context, result, callback);
                    }
                    else {
                        callback(err, result);
                    }
                });
            }
        });
    }
    onEnabled(handler) {
        this._onEnabled.unshift(handler);
        return this;
    }
    onFilter(handler) {
        this._onFilter.push(handler);
        return this;
    }
    isEnabled(context, callback) {
        let index = 0;
        let _that = this;
        function next(err, enabled) {
            if (index < _that._onEnabled.length && !err && enabled) {
                try {
                    _that._onEnabled[index++](context, next);
                }
                catch (e) {
                    callback(e, false);
                }
            }
            else {
                callback(err, enabled);
            }
        }
        next(null, true);
    }
    filter(context, result, callback) {
        let index = 0;
        let _that = this;
        function next(err, r) {
            if (index < _that._onFilter.length && !err) {
                try {
                    _that._onFilter[index++](context, r, next);
                }
                catch (e) {
                    callback(e, null);
                }
            }
            else {
                callback(err, r);
            }
        }
        next(null, result);
    }
}
exports.IntentRecognizer = IntentRecognizer;
