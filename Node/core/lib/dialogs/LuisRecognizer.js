"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const IntentRecognizer_1 = require("./IntentRecognizer");
const request = require("request");
const url = require("url");
class LuisRecognizer extends IntentRecognizer_1.IntentRecognizer {
    constructor(models) {
        super();
        if (typeof models == 'string') {
            this.models = { '*': models };
        }
        else {
            this.models = (models || {});
        }
    }
    onRecognize(context, callback) {
        let result = { score: 0.0, intent: null };
        if (context && context.message && context.message.text) {
            const locale = context.locale || '*';
            const dashPos = locale.indexOf('-');
            const parentLocale = dashPos > 0 ? locale.substr(0, dashPos) : '*';
            const model = this.models[locale] || this.models[parentLocale] || this.models['*'];
            if (model) {
                const utterance = context.message.text;
                LuisRecognizer.recognize(utterance, model, (err, intents, entities, compositeEntities) => {
                    if (!err) {
                        result.intents = intents;
                        result.entities = entities;
                        result.compositeEntities = compositeEntities;
                        var top;
                        intents.forEach((intent) => {
                            if (top) {
                                if (intent.score > top.score) {
                                    top = intent;
                                }
                            }
                            else {
                                top = intent;
                            }
                        });
                        if (top) {
                            result.score = top.score;
                            result.intent = top.intent;
                            switch (top.intent.toLowerCase()) {
                                case 'builtin.intent.none':
                                case 'none':
                                    result.score = 0.1;
                                    break;
                            }
                        }
                        callback(null, result);
                    }
                    else {
                        callback(err, null);
                    }
                });
            }
            else {
                callback(new Error("LUIS model not found for locale '" + locale + "'."), null);
            }
        }
        else {
            callback(null, result);
        }
    }
    static recognize(utterance, modelUrl, callback) {
        try {
            var uri = url.parse(modelUrl, true);
            uri.query['q'] = utterance || '';
            if (uri.search) {
                delete uri.search;
            }
            request.get(url.format(uri), (err, res, body) => {
                var result;
                try {
                    if (res && res.statusCode === 200) {
                        result = JSON.parse(body);
                        result.intents = result.intents || [];
                        result.entities = result.entities || [];
                        result.compositeEntities = result.compositeEntities || [];
                        if (result.topScoringIntent && result.intents.length == 0) {
                            result.intents.push(result.topScoringIntent);
                        }
                        if (result.intents.length == 1 && typeof result.intents[0].score !== 'number') {
                            result.intents[0].score = 1.0;
                        }
                    }
                    else {
                        err = new Error(body);
                    }
                }
                catch (e) {
                    err = e;
                }
                try {
                    if (!err) {
                        callback(null, result.intents, result.entities, result.compositeEntities);
                    }
                    else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m));
                    }
                }
                catch (e) {
                    console.error(e.toString());
                }
            });
        }
        catch (err) {
            callback(err instanceof Error ? err : new Error(err.toString()));
        }
    }
}
exports.LuisRecognizer = LuisRecognizer;
