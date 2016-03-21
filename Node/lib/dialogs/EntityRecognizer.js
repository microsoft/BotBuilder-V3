var utils = require('../utils');
var sprintf = require('sprintf-js');
var chrono = require('chrono-node');
var EntityRecognizer = (function () {
    function EntityRecognizer() {
    }
    EntityRecognizer.findEntity = function (entities, type) {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                return entities[i];
            }
        }
        return null;
    };
    EntityRecognizer.findAllEntities = function (entities, type) {
        var found = [];
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                found.push(entities[i]);
            }
        }
        return found;
    };
    EntityRecognizer.parseTime = function (entities) {
        if (typeof entities == 'string') {
            entities = EntityRecognizer.recognizeTime(entities);
        }
        return EntityRecognizer.resolveTime(entities);
    };
    EntityRecognizer.resolveTime = function (entities, timezoneOffset) {
        var now = new Date();
        var date;
        var time;
        entities.forEach(function (entity) {
            if (entity.resolution) {
                switch (entity.resolution.resolution_type) {
                    case 'builtin.datetime.date':
                        if (!date) {
                            date = entity.resolution.date;
                        }
                        break;
                    case 'builtin.datetime.time':
                        if (!time) {
                            time = entity.resolution.time;
                            if (time.length == 3) {
                                time = time + ':00:00';
                            }
                            else if (time.length == 6) {
                                time = time + ':00';
                            }
                        }
                        break;
                    case 'chrono.duration':
                        // Date is already calculated
                        var duration = entity;
                        return duration.resolution.start;
                }
            }
        });
        if (date || time) {
            // The user can just say "at 9am" so we'll use today if no date.
            if (!date) {
                date = utils.toDate8601(now);
            }
            if (time) {
                // Append time but adjust timezone. Default is to use bots timezone.
                if (typeof timezoneOffset !== 'number') {
                    timezoneOffset = now.getTimezoneOffset() / 60;
                }
                date = sprintf.sprintf('%s%s%s%02d:00', date, time, (timezoneOffset > 0 ? '-' : '+'), timezoneOffset);
            }
            return new Date(date);
        }
        return null;
    };
    EntityRecognizer.recognizeTime = function (utterance, refDate) {
        var response;
        try {
            var results = chrono.parse(utterance, refDate);
            if (results && results.length > 0) {
                var duration = results[0];
                response = {
                    type: 'chrono.duration',
                    entity: duration.text,
                    startIndex: duration.index,
                    endIndex: duration.index + duration.text.length,
                    resolution: {
                        resolution_type: 'chrono.duration',
                        start: duration.start.date()
                    }
                };
                if (duration.end) {
                    response.resolution.end = duration.end.date();
                }
                if (duration.ref) {
                    response.resolution.ref = duration.ref;
                }
                // Calculate a confidence score based on text coverage and call compareConfidence.
                response.score = duration.text.length / utterance.length;
            }
        }
        catch (err) {
            console.error('Error recognizing time: ' + err.toString());
            response = null;
        }
        return response;
    };
    EntityRecognizer.parseNumber = function (entities) {
        var entity;
        if (typeof entities == 'string') {
            entity = { type: 'text', entity: entities.trim() };
        }
        else {
            entity = EntityRecognizer.findEntity(entities, 'builtin.number');
        }
        if (entity) {
            var match = this.numberExp.exec(entity.entity);
            if (match) {
                return Number(match[0]);
            }
        }
        return undefined;
    };
    EntityRecognizer.parseBoolean = function (utterance) {
        utterance = utterance.trim();
        if (EntityRecognizer.yesExp.test(utterance)) {
            return true;
        }
        else if (EntityRecognizer.noExp.test(utterance)) {
            return false;
        }
        return undefined;
    };
    EntityRecognizer.findBestMatch = function (choices, utterance, threshold) {
        if (threshold === void 0) { threshold = 0.6; }
        var best;
        var matches = EntityRecognizer.findAllMatches(choices, utterance, threshold);
        matches.forEach(function (value) {
            if (!best || value.score > best.score) {
                best = value;
            }
        });
        return best;
    };
    EntityRecognizer.findAllMatches = function (choices, utterance, threshold) {
        if (threshold === void 0) { threshold = 0.6; }
        var matches = [];
        utterance = utterance.trim().toLowerCase();
        var tokens = utterance.split(' ');
        EntityRecognizer.expandChoices(choices).forEach(function (choice, index) {
            var score = 0.0;
            var value = choice.trim().toLowerCase();
            if (value.indexOf(utterance) >= 0) {
                score = utterance.length / value.length;
            }
            else if (utterance.indexOf(value) >= 0) {
                score = value.length / utterance.length;
            }
            else {
                var matched = '';
                tokens.forEach(function (token) {
                    if (value.indexOf(token) >= 0) {
                        matched += token;
                    }
                });
                score = matched.length / value.length;
            }
            if (score > threshold) {
                matches.push({ index: index, entity: choice, score: score });
            }
        });
        return matches;
    };
    EntityRecognizer.expandChoices = function (choices) {
        if (!choices) {
            return [];
        }
        else if (Array.isArray(choices)) {
            return choices;
        }
        else if (typeof choices == 'string') {
            return choices.split('|');
        }
        else if (typeof choices == 'object') {
            var list = [];
            for (var key in choices) {
                list.push(key);
            }
            return list;
        }
        else {
            return [choices.toString()];
        }
    };
    EntityRecognizer.yesExp = /^(1|y|yes|yep|sure|ok|true)\z/i;
    EntityRecognizer.noExp = /^(0|n|no|nope|not|false)\z/i;
    EntityRecognizer.numberExp = /[+-]?(?:\d+\.?\d*|\d*\.?\d+)/;
    return EntityRecognizer;
})();
exports.EntityRecognizer = EntityRecognizer;
