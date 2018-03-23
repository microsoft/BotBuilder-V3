"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const IntentRecognizer_1 = require("./IntentRecognizer");
const RegExpRecognizer_1 = require("./RegExpRecognizer");
class LocalizedRegExpRecognizer extends IntentRecognizer_1.IntentRecognizer {
    constructor(intent, key, namespace) {
        super();
        this.intent = intent;
        this.key = key;
        this.namespace = namespace;
        this.recognizers = {};
    }
    onRecognize(context, callback) {
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
    }
}
exports.LocalizedRegExpRecognizer = LocalizedRegExpRecognizer;
