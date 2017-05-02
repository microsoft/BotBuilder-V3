"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var IntentRecognizer_1 = require("./IntentRecognizer");
var RegExpRecognizer_1 = require("./RegExpRecognizer");
var LocalizedRegExpRecognizer = (function (_super) {
    __extends(LocalizedRegExpRecognizer, _super);
    function LocalizedRegExpRecognizer(intent, key, namespace) {
        var _this = _super.call(this) || this;
        _this.intent = intent;
        _this.key = key;
        _this.namespace = namespace;
        _this.recognizers = {};
        return _this;
    }
    LocalizedRegExpRecognizer.prototype.onRecognize = function (context, callback) {
        var locale = context.preferredLocale();
        var recognizer = this.recognizers[locale];
        if (!recognizer) {
            var pattern = context.localizer.trygettext(locale, this.key, this.namespace);
            if (pattern) {
                var exp = new RegExp(pattern, 'i');
                this.recognizers[locale] = recognizer = new RegExpRecognizer_1.RegExpRecognizer(this.intent, exp);
            }
        }
        if (recognizer) {
            recognizer.recognize(context, callback);
        }
        else {
            callback(null, { score: 0.0, intent: null });
        }
    };
    return LocalizedRegExpRecognizer;
}(IntentRecognizer_1.IntentRecognizer));
exports.LocalizedRegExpRecognizer = LocalizedRegExpRecognizer;
