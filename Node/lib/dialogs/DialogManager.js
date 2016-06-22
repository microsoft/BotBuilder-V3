var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dlg = require('./Dialog');
var request = require('request');
var url = require('url');
var actions = require('./DialogAction');
var DialogManager = (function (_super) {
    __extends(DialogManager, _super);
    function DialogManager(settings) {
        _super.call(this);
        this.settings = settings;
        if (!this.settings.endpoint) {
            this.settings.endpoint = 'https://www.bingapis.com';
        }
    }
    DialogManager.prototype.replyReceived = function (session, recognizeResult) {
        var _this = this;
        var msg = session.message;
        this.process(msg.text, msg.address.conversation.id, function (err, response, body) {
            if (!err) {
                var handled = false;
                if (body && body.state) {
                    switch (body.state) {
                        case 'Completed':
                        case 'PartialResponse':
                        case 'ChatAnswer':
                            if (body.text) {
                                handled = true;
                                session.send(body.text);
                            }
                            break;
                    }
                }
                if (!handled && _this.defaultHandler) {
                    try {
                        _this.defaultHandler(session, body);
                    }
                    catch (e) {
                        session.error(e);
                    }
                }
            }
            else {
                session.error(err);
            }
        });
    };
    DialogManager.prototype.dialogResumed = function (session, result) {
        if (this.defaultHandler) {
            try {
                this.defaultHandler(session, result);
            }
            catch (e) {
                session.error(e);
            }
        }
        else {
            _super.prototype.dialogResumed.call(this, session, result);
        }
    };
    DialogManager.prototype.process = function (utterance, conversationId, callback) {
        try {
            var path = '/api/v5/dialog/agents/' +
                encodeURIComponent(this.settings.agentId) +
                '/conversations/' +
                encodeURIComponent(conversationId) +
                '/messages?appid=' +
                encodeURIComponent(this.settings.appId) +
                '&includeSemanticFrames=false&includeTaskFrameStates=false';
            var options = {
                method: 'POST',
                url: url.resolve(this.settings.endpoint, path),
                body: { Text: utterance },
                json: true
            };
            request(options, function (err, response, body) {
                try {
                    if (!err) {
                        if (response.statusCode < 400) {
                            callback(null, response, body);
                        }
                        else {
                            var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                            callback(new Error(txt), response, null);
                        }
                    }
                    else {
                        callback(err, null, null);
                    }
                }
                catch (e) {
                    console.error(e.toString());
                }
            });
        }
        catch (err) {
            callback(err instanceof Error ? err : new Error(err.toString()), null, null);
        }
    };
    DialogManager.prototype.onDefault = function (dialogId, dialogArgs) {
        if (Array.isArray(dialogId)) {
            this.defaultHandler = actions.waterfall(dialogId);
        }
        else if (typeof dialogId === 'string') {
            this.defaultHandler = actions.DialogAction.beginDialog(dialogId, dialogArgs);
        }
        else {
            this.defaultHandler = actions.waterfall([dialogId]);
        }
        return this;
    };
    return DialogManager;
})(dlg.Dialog);
exports.DialogManager = DialogManager;
