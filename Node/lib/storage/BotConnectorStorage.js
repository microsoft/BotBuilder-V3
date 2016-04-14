var request = require('request');
var BotConnectorStorage = (function () {
    function BotConnectorStorage(options) {
        this.options = options;
    }
    BotConnectorStorage.prototype.get = function (id, callback) {
        var settings = this.options;
        var options = {
            url: settings.endpoint + '/bot/v1.0/bots' + id
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
            try {
                var data;
                if (!err && typeof body === 'string') {
                    data = JSON.parse(body);
                }
                callback(err, data);
            }
            catch (e) {
                callback(e instanceof Error ? e : new Error(e.toString()), null);
            }
        });
    };
    BotConnectorStorage.prototype.save = function (id, data, callback) {
        var settings = this.options;
        var options = {
            url: settings.endpoint + '/bot/v1.0/bots' + id,
            body: data
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
        request.post(options, callback);
    };
    return BotConnectorStorage;
})();
exports.BotConnectorStorage = BotConnectorStorage;
