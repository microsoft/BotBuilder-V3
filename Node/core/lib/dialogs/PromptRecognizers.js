"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const EntityRecognizer_1 = require("./EntityRecognizer");
const consts = require("../consts");
const breakingChars = " \n\r~`!@#$%^&*()-+={}|[]\\:\";'<>?,./";
class PromptRecognizers {
    static recognizeLocalizedRegExp(context, expId, namespace) {
        let key = namespace + ':' + expId;
        let entities = [];
        const locale = context.preferredLocale();
        const utterance = context.message.text ? context.message.text.trim() : '';
        let cache = this.expCache[key];
        if (!cache) {
            this.expCache[key] = cache = {};
        }
        if (!cache.hasOwnProperty(locale)) {
            cache[locale] = new RegExp(context.localizer.gettext(locale, expId, namespace), 'ig');
        }
        let matches = matchAll(cache[locale], utterance);
        matches.forEach((value) => {
            entities.push({
                type: consts.Entities.String,
                entity: value,
                score: PromptRecognizers.calculateScore(utterance, value)
            });
        });
        return entities;
    }
    static recognizeLocalizedChoices(context, listId, namespace, options) {
        let key = namespace + ':' + listId;
        let entities = [];
        const locale = context.preferredLocale();
        const utterance = context.message.text ? context.message.text.trim() : '';
        let cache = this.choiceCache[key];
        if (!cache) {
            this.expCache[key] = cache = {};
        }
        if (!cache.hasOwnProperty(locale)) {
            let list = context.localizer.gettext(locale, listId, namespace);
            cache[locale] = PromptRecognizers.toChoices(list);
        }
        return PromptRecognizers.recognizeChoices(context.message.text, cache[locale], options);
    }
    static toChoices(list) {
        let choices = [];
        if (list) {
            list.split('|').forEach((value, index) => {
                let pos = value.indexOf('=');
                if (pos > 0) {
                    choices.push({
                        value: value.substr(0, pos),
                        synonyms: value.substr(pos + 1).split(',')
                    });
                }
                else {
                    choices.push({
                        value: value,
                        synonyms: []
                    });
                }
            });
        }
        return choices;
    }
    static recognizeBooleans(context) {
        let entities = [];
        let results = PromptRecognizers.recognizeLocalizedChoices(context, 'boolean_choices', consts.Library.system, { excludeValue: true });
        if (results) {
            results.forEach((result) => {
                let value = (result.entity.entity === 'true');
                entities.push({
                    type: consts.Entities.Boolean,
                    entity: value,
                    score: result.score
                });
            });
        }
        return entities;
    }
    static recognizeNumbers(context, options) {
        function addEntity(n, score) {
            if ((typeof options.minValue !== 'number' || n >= options.minValue) &&
                (typeof options.maxValue !== 'number' || n <= options.maxValue) &&
                (!options.integerOnly || Math.floor(n) == n)) {
                entities.push({
                    type: consts.Entities.Number,
                    entity: n,
                    score: score
                });
            }
        }
        options = options || {};
        let entities = [];
        let matches = PromptRecognizers.recognizeLocalizedRegExp(context, 'number_exp', consts.Library.system);
        if (matches) {
            matches.forEach((entity) => {
                let n = Number(entity.entity);
                addEntity(n, entity.score);
            });
        }
        let results = PromptRecognizers.recognizeLocalizedChoices(context, 'number_terms', consts.Library.system, { excludeValue: true });
        if (results) {
            results.forEach((result) => {
                let n = Number(result.entity.entity);
                addEntity(n, result.score);
            });
        }
        return entities;
    }
    static recognizeOrdinals(context) {
        let entities = [];
        let results = PromptRecognizers.recognizeLocalizedChoices(context, 'number_ordinals', consts.Library.system, { excludeValue: true });
        if (results) {
            results.forEach((result) => {
                let n = Number(result.entity.entity);
                entities.push({
                    type: consts.Entities.Number,
                    entity: n,
                    score: result.score
                });
            });
        }
        results = PromptRecognizers.recognizeLocalizedChoices(context, 'number_reverse_ordinals', consts.Library.system, { excludeValue: true });
        if (results) {
            results.forEach((result) => {
                let n = Number(result.entity.entity);
                entities.push({
                    type: consts.Entities.Number,
                    entity: n,
                    score: result.score
                });
            });
        }
        return entities;
    }
    static recognizeTimes(context, options) {
        options = options || {};
        let refData = options.refDate ? new Date(options.refDate) : null;
        let entities = [];
        const utterance = context.message.text ? context.message.text.trim() : '';
        let entity = EntityRecognizer_1.EntityRecognizer.recognizeTime(utterance, refData);
        if (entity) {
            entity.score = PromptRecognizers.calculateScore(utterance, entity.entity);
            entities.push(entity);
        }
        return entities;
    }
    static recognizeChoices(utterance, choices, options) {
        options = options || {};
        let entities = [];
        choices.forEach((choice, index) => {
            let values = Array.isArray(choice.synonyms) ? choice.synonyms : (choice.synonyms || '').split('|');
            if (!options.excludeValue) {
                values.push(choice.value);
            }
            if (choice.action && !options.excludeAction) {
                let action = choice.action;
                if (action.title && action.title !== choice.value) {
                    values.push(action.title);
                }
                if (action.value && action.value !== choice.value && action.value !== action.title) {
                    values.push(action.value);
                }
            }
            let match = PromptRecognizers.findTopEntity(PromptRecognizers.recognizeValues(utterance, values, options));
            if (match) {
                entities.push({
                    type: consts.Entities.Match,
                    score: match.score,
                    entity: {
                        index: index,
                        entity: choice.value,
                        score: match.score
                    }
                });
            }
        });
        return entities;
    }
    static recognizeValues(utterance, values, options) {
        function indexOfToken(token, startPos) {
            for (let i = startPos; i < tokens.length; i++) {
                if (tokens[i] === token) {
                    return i;
                }
            }
            return -1;
        }
        function matchValue(vTokens, startPos) {
            let matched = 0;
            let totalDeviation = 0;
            vTokens.forEach((token) => {
                let pos = indexOfToken(token, startPos);
                if (pos >= 0) {
                    let distance = matched > 0 ? pos - startPos : 0;
                    if (distance <= maxDistance) {
                        matched++;
                        totalDeviation += distance;
                        startPos = pos + 1;
                    }
                }
            });
            let score = 0.0;
            if (matched > 0 && (matched == vTokens.length || options.allowPartialMatches)) {
                let completeness = matched / vTokens.length;
                let accuracy = completeness * (matched / (matched + totalDeviation));
                let initialScore = accuracy * (matched / tokens.length);
                score = 0.4 + (0.6 * initialScore);
            }
            return score;
        }
        options = options || {};
        let entities = [];
        let text = utterance.trim().toLowerCase();
        let tokens = tokenize(text);
        let maxDistance = options.hasOwnProperty('maxTokenDistance') ? options.maxTokenDistance : 2;
        values.forEach((value, index) => {
            if (typeof value === 'string') {
                let topScore = 0.0;
                let vTokens = tokenize(value.trim().toLowerCase());
                for (let i = 0; i < tokens.length; i++) {
                    let score = matchValue(vTokens, i);
                    if (score > topScore) {
                        topScore = score;
                    }
                }
                if (topScore > 0.0) {
                    entities.push({
                        type: consts.Entities.Number,
                        entity: index,
                        score: topScore
                    });
                }
            }
            else {
                let matches = value.exec(text) || [];
                if (matches.length > 0) {
                    entities.push({
                        type: consts.Entities.Number,
                        entity: index,
                        score: PromptRecognizers.calculateScore(text, matches[0])
                    });
                }
            }
        });
        return entities;
    }
    static findTopEntity(entities) {
        let top = null;
        if (entities) {
            entities.forEach((entity) => {
                if (!top || entity.score > top.score) {
                    top = entity;
                }
            });
        }
        return top;
    }
    static calculateScore(utterance, entity, max = 1.0, min = 0.5) {
        return Math.min(min + (entity.length / utterance.length), max);
    }
}
PromptRecognizers.numOrdinals = {};
PromptRecognizers.expCache = {};
PromptRecognizers.choiceCache = {};
exports.PromptRecognizers = PromptRecognizers;
function matchAll(exp, text) {
    exp.lastIndex = 0;
    let matches = [];
    let match;
    while ((match = exp.exec(text)) != null) {
        matches.push(match[0]);
    }
    return matches;
}
function tokenize(text) {
    let tokens = [];
    if (text && text.length > 0) {
        let token = '';
        for (let i = 0; i < text.length; i++) {
            const chr = text[i];
            if (breakingChars.indexOf(chr) >= 0) {
                if (token.length > 0) {
                    tokens.push(token);
                }
                token = '';
            }
            else {
                token += chr;
            }
        }
        if (token.length > 0) {
            tokens.push(token);
        }
    }
    return tokens;
}
