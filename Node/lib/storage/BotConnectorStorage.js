var request = require('request');
var BotConnectorStorage = (function () {
    function BotConnectorStorage(options) {
        this.options = options;
    }
    BotConnectorStorage.prototype.get = function (address, callback) {
        var ops = 2;
        var settings = this.options;
        var data = {};
        function read(path, field) {
            if (path) {
                var options = {
                    url: settings.endpoint + '/bot/v1.0/bots' + path
                };
                if (settings.appId && settings.appSecret) {
                    options.auth = {
                        username: settings.appId,
                        password: settings.appSecret
                    };
                    options.headers = {
                        'Ocp-Apim-Subscription-Key': settings.appSecret
                    };
                }
                request.get(options, function (err, response, body) {
                    if (!err) {
                        try {
                            data[field + 'Hash'] = body;
                            data[field] = typeof body === 'string' ? JSON.parse(body) : null;
                        }
                        catch (e) {
                            err = e instanceof Error ? e : new Error(e.toString());
                        }
                    }
                    if (callback && (err || --ops == 0)) {
                        callback(err, data);
                        callback = null;
                    }
                });
            }
            else if (callback && --ops == 0) {
                callback(null, data);
            }
        }
        var userPath = address.userId ? '/users/' + address.userId : null;
        var convoPath = address.conversationId ? '/conversations/' + address.conversationId + userPath : null;
        read(userPath, 'userData');
        read(convoPath, 'conversationData');
    };
    BotConnectorStorage.prototype.save = function (address, data, callback) {
        var ops = 2;
        var settings = this.options;
        function write(path, field) {
            if (path) {
                var err;
                var body;
                var hashField = field + 'Hash';
                try {
                    body = JSON.stringify(data[field]);
                }
                catch (e) {
                    err = e instanceof Error ? e : new Error(e.toString());
                }
                if (!err && (!data[hashField] || body !== data[hashField])) {
                    data[hashField] = body;
                    var options = {
                        url: settings.endpoint + '/bot/v1.0/bots' + path,
                        body: body
                    };
                    if (settings.appId && settings.appSecret) {
                        options.auth = {
                            username: settings.appId,
                            password: settings.appSecret
                        };
                        options.headers = {
                            'Ocp-Apim-Subscription-Key': settings.appSecret
                        };
                    }
                    request.post(options, function (err) {
                        if (callback && (err || --ops == 0)) {
                            callback(err);
                            callback = null;
                        }
                    });
                }
                else if (callback && (err || --ops == 0)) {
                    callback(err);
                    callback = null;
                }
            }
            else if (callback && --ops == 0) {
                callback(null);
            }
        }
        var userPath = address.userId ? '/users/' + address.userId : null;
        var convoPath = address.conversationId ? '/conversations/' + address.conversationId + userPath : null;
        write(userPath, 'userData');
        write(convoPath, 'conversationData');
    };
    return BotConnectorStorage;
})();
exports.BotConnectorStorage = BotConnectorStorage;
