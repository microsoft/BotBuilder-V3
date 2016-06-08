var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var events = require('events');
var request = require('request');
var async = require('async');
var url = require('url');
var BotConnector = (function (_super) {
    __extends(BotConnector, _super);
    function BotConnector() {
        _super.apply(this, arguments);
    }
    BotConnector.prototype.listen = function () {
        var _this = this;
        return function (req, res) {
            if (req.body) {
                _this.dispatch(req.body, res);
            }
            else {
                var requestData = '';
                req.on('data', function (chunk) {
                    requestData += chunk;
                });
                req.on('end', function () {
                    var body = JSON.parse(requestData);
                    _this.dispatch(body, res);
                });
            }
        };
    };
    BotConnector.prototype.dispatch = function (messages, res) {
        var _this = this;
        var list = Array.isArray(messages) ? messages : [messages];
        list.forEach(function (msg) {
            try {
                var address = {};
                moveFields(msg, address, toAddress);
                msg.address = address;
                if (msg.type && msg.type.toLowerCase().indexOf('message') == 0) {
                    _this.handler([msg]);
                }
                else {
                    _this.emit(msg.type, msg);
                }
            }
            catch (e) {
                console.error(e.toString());
            }
        });
        res.status(202);
        res.end();
    };
    BotConnector.prototype.onMessage = function (handler) {
        this.handler = handler;
    };
    BotConnector.prototype.send = function (messages, cb) {
        var _this = this;
        var conversationId;
        async.eachSeries(messages, function (msg, cb) {
            try {
                var address = msg.address;
                if (address && address.serviceUrl) {
                    delete msg.address;
                    if (!address.conversation && conversationId) {
                        address.conversation = { id: conversationId };
                    }
                    _this.postMessage(address, msg, function (err, id) {
                        if (!err && id) {
                            conversationId = id;
                        }
                        cb(err);
                    });
                }
                else {
                    cb(new Error('Message missing address or serviceUrl.'));
                }
            }
            catch (e) {
                cb(e);
            }
        }, function (err) {
            cb(err, conversationId);
        });
    };
    BotConnector.prototype.postMessage = function (address, msg, cb) {
        var path = '/api/v3/conversations';
        if (address.conversation && address.conversation.id) {
            path += '/' + encodeURIComponent(address.conversation.id) + '/activities';
            if (address.id) {
                path += '/' + encodeURIComponent(address.id);
            }
        }
        msg['from'] = address.bot;
        msg['recipient'] = address.user;
        var options = {
            method: 'POST',
            url: url.resolve(address.serviceUrl, path),
            body: msg,
            json: true
        };
        request(options, function (err, response, body) {
            var conversationId;
            if (!err && body) {
                try {
                    var obj = typeof body === 'string' ? JSON.parse(body) : body;
                    if (obj.hasOwnProperty('conversationId')) {
                        conversationId = obj['conversationId'];
                    }
                }
                catch (e) {
                    console.error('Error parsing channel response: ' + e.toString());
                }
            }
            cb(err, conversationId);
        });
    };
    return BotConnector;
})(events.EventEmitter);
exports.BotConnector = BotConnector;
var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'to': 'conversation',
    'recipient': 'bot',
    'serviceUrl': 'serviceUrl'
};
function moveFields(frm, to, map) {
    if (frm && to) {
        for (var key in map) {
            if (frm.hasOwnProperty(key)) {
                to[map[key]] = frm[key];
                delete frm[key];
            }
        }
    }
}
