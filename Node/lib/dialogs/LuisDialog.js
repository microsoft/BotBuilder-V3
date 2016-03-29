var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var intent = require('./IntentDialog');
var utils = require('../utils');
var request = require('request');
var sprintf = require('sprintf-js');
var LuisDialog = (function (_super) {
    __extends(LuisDialog, _super);
    function LuisDialog(serviceUri) {
        _super.call(this);
        this.serviceUri = serviceUri;
    }
    LuisDialog.prototype.recognizeIntents = function (language, utterance, callback) {
        LuisDialog.recognize(utterance, this.serviceUri, callback);
    };
    LuisDialog.recognize = function (utterance, serviceUri, callback) {
        var uri = serviceUri.trim();
        if (uri.lastIndexOf('&q=') != uri.length - 3) {
            uri += '&q=';
        }
        uri += encodeURIComponent(utterance || '');
        request.get(uri, function (err, res, body) {
            try {
                if (!err) {
                    var result = JSON.parse(body);
                    if (result.intents.length == 1 && !result.intents[0].hasOwnProperty('score')) {
                        result.intents[0].score = 1.0;
                    }
                    callback(null, result.intents, result.entities);
                }
                else {
                    callback(err);
                }
            }
            catch (e) {
                callback(e);
            }
        });
    };
    return LuisDialog;
})(intent.IntentDialog);
exports.LuisDialog = LuisDialog;
var LuisEntityResolver = (function () {
    function LuisEntityResolver() {
    }
    LuisEntityResolver.findEntity = function (entities, type) {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                return entities[i];
            }
        }
        return null;
    };
    LuisEntityResolver.findAllEntities = function (entities, type) {
        var found = [];
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                found.push(entities[i]);
            }
        }
        return found;
    };
    LuisEntityResolver.resolveDate = function (entities, timezoneOffset) {
        var now = new Date();
        var date;
        var time;
        for (var i = 0; i < entities.length; i++) {
            var entity = entities[i];
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
                }
            }
        }
        if (date || time) {
            if (!date) {
                date = utils.toDate8601(now);
            }
            if (time) {
                if (typeof timezoneOffset !== 'number') {
                    timezoneOffset = now.getTimezoneOffset() / 60;
                }
                date = sprintf.sprintf('%s%s%s%02d:00', date, time, (timezoneOffset > 0 ? '-' : '+'), timezoneOffset);
            }
            return new Date(date);
        }
        else {
            return null;
        }
    };
    return LuisEntityResolver;
})();
exports.LuisEntityResolver = LuisEntityResolver;
