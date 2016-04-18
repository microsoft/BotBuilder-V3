var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var intent = require('./IntentDialog');
var request = require('request');
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
            var calledCallback = false;
            try {
                if (!err) {
                    var result = JSON.parse(body);
                    if (result.intents.length == 1 && typeof result.intents[0].score !== 'number') {
                        result.intents[0].score = 1.0;
                    }
                    calledCallback = true;
                    callback(null, result.intents, result.entities);
                }
                else {
                    calledCallback = true;
                    callback(err);
                }
            }
            catch (e) {
                if (!calledCallback) {
                    callback(e);
                }
                else {
                    console.error(e.toString());
                }
            }
        });
    };
    return LuisDialog;
})(intent.IntentDialog);
exports.LuisDialog = LuisDialog;
