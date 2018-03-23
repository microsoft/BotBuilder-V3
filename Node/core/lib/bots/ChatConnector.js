"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const OpenIdMetadata_1 = require("./OpenIdMetadata");
const utils = require("../utils");
const logger = require("../logger");
const consts = require("../consts");
const request = require("request");
const async = require("async");
const jwt = require("jsonwebtoken");
const zlib = require("zlib");
const urlJoin = require("url-join");
var pjson = require('../../package.json');
var MAX_DATA_LENGTH = 65000;
var USER_AGENT = "Microsoft-BotFramework/3.1 (BotBuilder Node.js/" + pjson.version + ")";
var StateApiDreprecatedMessage = "The Bot State API is deprecated.  Please refer to https://aka.ms/I6swrh for details on how to replace with your own storage.";
class ChatConnector {
    constructor(settings = {}) {
        this.settings = settings;
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
                refreshScope: 'https://api.botframework.com/.default',
                botConnectorOpenIdMetadata: this.settings.openIdMetadata || 'https://login.botframework.com/v1/.well-known/openidconfiguration',
                botConnectorIssuer: 'https://api.botframework.com',
                botConnectorAudience: this.settings.appId,
                emulatorOpenIdMetadata: 'https://login.microsoftonline.com/botframework.com/v2.0/.well-known/openid-configuration',
                emulatorAudience: this.settings.appId,
                emulatorAuthV31IssuerV1: 'https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/',
                emulatorAuthV31IssuerV2: 'https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0',
                emulatorAuthV32IssuerV1: 'https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/',
                emulatorAuthV32IssuerV2: 'https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0',
                stateEndpoint: this.settings.stateEndpoint || 'https://state.botframework.com'
            };
        }
        this.botConnectorOpenIdMetadata = new OpenIdMetadata_1.OpenIdMetadata(this.settings.endpoint.botConnectorOpenIdMetadata);
        this.emulatorOpenIdMetadata = new OpenIdMetadata_1.OpenIdMetadata(this.settings.endpoint.emulatorOpenIdMetadata);
    }
    listen() {
        function defaultNext() { }
        return (req, res, next) => {
            if (req.body) {
                this.verifyBotFramework(req, res, next || defaultNext);
            }
            else {
                var requestData = '';
                req.on('data', (chunk) => {
                    requestData += chunk;
                });
                req.on('end', () => {
                    try {
                        req.body = JSON.parse(requestData);
                    }
                    catch (err) {
                        logger.error('ChatConnector: receive - invalid request data received.');
                        res.send(400);
                        res.end();
                        return;
                    }
                    this.verifyBotFramework(req, res, next || defaultNext);
                });
            }
        };
    }
    verifyBotFramework(req, res, next) {
        var token;
        var isEmulator = req.body['channelId'] === 'emulator';
        var authHeaderValue = req.headers ? req.headers['authorization'] || req.headers['Authorization'] : null;
        if (authHeaderValue) {
            var auth = authHeaderValue.trim().split(' ');
            if (auth.length == 2 && auth[0].toLowerCase() == 'bearer') {
                token = auth[1];
            }
        }
        if (token) {
            let decoded = jwt.decode(token, { complete: true });
            var verifyOptions;
            var openIdMetadata;
            const algorithms = ['RS256', 'RS384', 'RS512'];
            if (isEmulator) {
                if ((decoded.payload.ver === '2.0' && decoded.payload.azp !== this.settings.appId) ||
                    (decoded.payload.ver !== '2.0' && decoded.payload.appid !== this.settings.appId)) {
                    logger.error('ChatConnector: receive - invalid token. Requested by unexpected app ID.');
                    res.status(403);
                    res.end();
                    next();
                    return;
                }
                let issuer;
                if (decoded.payload.ver === '1.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV31IssuerV1) {
                    issuer = this.settings.endpoint.emulatorAuthV31IssuerV1;
                }
                else if (decoded.payload.ver === '2.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV31IssuerV2) {
                    issuer = this.settings.endpoint.emulatorAuthV31IssuerV2;
                }
                else if (decoded.payload.ver === '1.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV32IssuerV1) {
                    issuer = this.settings.endpoint.emulatorAuthV32IssuerV1;
                }
                else if (decoded.payload.ver === '2.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV32IssuerV2) {
                    issuer = this.settings.endpoint.emulatorAuthV32IssuerV2;
                }
                if (issuer) {
                    openIdMetadata = this.emulatorOpenIdMetadata;
                    verifyOptions = {
                        algorithms: algorithms,
                        issuer: issuer,
                        audience: this.settings.endpoint.emulatorAudience,
                        clockTolerance: 300
                    };
                }
            }
            if (!verifyOptions) {
                openIdMetadata = this.botConnectorOpenIdMetadata;
                verifyOptions = {
                    issuer: this.settings.endpoint.botConnectorIssuer,
                    audience: this.settings.endpoint.botConnectorAudience,
                    clockTolerance: 300
                };
            }
            openIdMetadata.getKey(decoded.header.kid, key => {
                if (key) {
                    try {
                        jwt.verify(token, key.key, verifyOptions);
                        if (typeof req.body.channelId !== 'undefined' &&
                            typeof key.endorsements !== 'undefined' &&
                            key.endorsements.lastIndexOf(req.body.channelId) === -1) {
                            const errorDescription = `channelId in req.body: ${req.body.channelId} didn't match the endorsements: ${key.endorsements.join(',')}.`;
                            logger.error(`ChatConnector: receive - endorsements validation failure. ${errorDescription}`);
                            throw new Error(errorDescription);
                        }
                        if (typeof decoded.payload.serviceurl !== 'undefined' &&
                            typeof req.body.serviceUrl !== 'undefined' &&
                            decoded.payload.serviceurl !== req.body.serviceUrl) {
                            const errorDescription = `ServiceUrl in payload of token: ${decoded.payload.serviceurl} didn't match the request's serviceurl: ${req.body.serviceUrl}.`;
                            logger.error(`ChatConnector: receive - serviceurl mismatch. ${errorDescription}`);
                            throw new Error(errorDescription);
                        }
                    }
                    catch (err) {
                        logger.error('ChatConnector: receive - invalid token. Check bot\'s app ID & Password.');
                        res.send(403, err);
                        res.end();
                        next();
                        return;
                    }
                    this.dispatch(req.body, res, next);
                }
                else {
                    logger.error('ChatConnector: receive - invalid signing key or OpenId metadata document.');
                    res.status(500);
                    res.end();
                    next();
                    return;
                }
            });
        }
        else if (isEmulator && !this.settings.appId && !this.settings.appPassword) {
            logger.warn(req.body, 'ChatConnector: receive - emulator running without security enabled.');
            this.dispatch(req.body, res, next);
        }
        else {
            logger.error('ChatConnector: receive - no security token sent.');
            res.status(401);
            res.end();
            next();
        }
    }
    onEvent(handler) {
        this.onEventHandler = handler;
    }
    onInvoke(handler) {
        this.onInvokeHandler = handler;
    }
    send(messages, done) {
        let addresses = [];
        async.forEachOfSeries(messages, (msg, idx, cb) => {
            try {
                if (msg.type == 'delay') {
                    setTimeout(cb, msg.value);
                }
                else {
                    const addressExists = !!msg.address;
                    const serviceUrlExists = addressExists && !!msg.address.serviceUrl;
                    if (serviceUrlExists) {
                        this.postMessage(msg, (idx == messages.length - 1), (err, address) => {
                            addresses.push(address);
                            cb(err);
                        });
                    }
                    else {
                        const msg = `Message is missing ${addressExists ? 'address and serviceUrl' : 'serviceUrl'} `;
                        logger.error(`ChatConnector: send - ${msg}`);
                        cb(new Error(msg));
                    }
                }
            }
            catch (e) {
                cb(e);
            }
        }, (err) => done(err, !err ? addresses : null));
    }
    startConversation(address, done) {
        if (address && address.user && address.bot && address.serviceUrl) {
            var options = {
                method: 'POST',
                url: urlJoin(address.serviceUrl, '/v3/conversations'),
                body: {
                    bot: address.bot,
                    members: address.members || [address.user]
                },
                json: true
            };
            if (address.activity) {
                options.body.activity = address.activity;
            }
            if (address.channelData) {
                options.body.channelData = address.channelData;
            }
            if (address.isGroup !== undefined) {
                options.body.isGroup = address.isGroup;
            }
            if (address.topicName) {
                options.body.topicName = address.topicName;
            }
            this.authenticatedRequest(options, (err, response, body) => {
                var adr;
                if (!err) {
                    try {
                        var obj = typeof body === 'string' ? JSON.parse(body) : body;
                        if (obj && obj.hasOwnProperty('id')) {
                            adr = utils.clone(address);
                            adr.conversation = { id: obj['id'] };
                            if (obj['serviceUrl']) {
                                adr.serviceUrl = obj['serviceUrl'];
                            }
                            if (adr.id) {
                                delete adr.id;
                            }
                        }
                        else {
                            err = new Error('Failed to start conversation: no conversation ID returned.');
                        }
                    }
                    catch (e) {
                        err = e instanceof Error ? e : new Error(e.toString());
                    }
                }
                if (err) {
                    logger.error('ChatConnector: startConversation - error starting conversation.');
                }
                done(err, adr);
            });
        }
        else {
            logger.error('ChatConnector: startConversation - address is invalid.');
            done(new Error('Invalid address.'));
        }
    }
    update(message, done) {
        let address = message.address;
        if (message.address && address.serviceUrl) {
            message.id = address.id;
            this.postMessage(message, true, done, 'PUT');
        }
        else {
            logger.error('ChatConnector: updateMessage - message is missing address or serviceUrl.');
            done(new Error('Message missing address or serviceUrl.'), null);
        }
    }
    delete(address, done) {
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) +
            '/activities/' + encodeURIComponent(address.id);
        var options = {
            method: 'DELETE',
            url: urlJoin(address.serviceUrl, path),
            json: true
        };
        this.authenticatedRequest(options, (err, response, body) => done(err));
    }
    getData(context, callback) {
        try {
            console.warn(StateApiDreprecatedMessage);
            var root = this.getStoragePath(context.address);
            var list = [];
            if (context.userId) {
                if (context.persistUserData) {
                    list.push({
                        field: 'userData',
                        url: root + '/users/' + encodeURIComponent(context.userId)
                    });
                }
                if (context.conversationId) {
                    list.push({
                        field: 'privateConversationData',
                        url: root + '/conversations/' + encodeURIComponent(context.conversationId) +
                            '/users/' + encodeURIComponent(context.userId)
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                list.push({
                    field: 'conversationData',
                    url: root + '/conversations/' + encodeURIComponent(context.conversationId)
                });
            }
            var data = {};
            async.each(list, (entry, cb) => {
                var options = {
                    method: 'GET',
                    url: entry.url,
                    json: true
                };
                this.authenticatedRequest(options, (err, response, body) => {
                    if (!err && body) {
                        var botData = body.data ? body.data : {};
                        if (typeof botData === 'string') {
                            zlib.gunzip(new Buffer(botData, 'base64'), (err, result) => {
                                if (!err) {
                                    try {
                                        var txt = result.toString();
                                        data[entry.field + 'Hash'] = txt;
                                        data[entry.field] = JSON.parse(txt);
                                    }
                                    catch (e) {
                                        err = e;
                                    }
                                }
                                cb(err);
                            });
                        }
                        else {
                            try {
                                data[entry.field + 'Hash'] = JSON.stringify(botData);
                                data[entry.field] = botData;
                            }
                            catch (e) {
                                err = e;
                            }
                            cb(err);
                        }
                    }
                    else {
                        cb(err);
                    }
                });
            }, (err) => {
                if (!err) {
                    callback(null, data);
                }
                else {
                    var m = err.toString();
                    callback(err instanceof Error ? err : new Error(m), null);
                }
            });
        }
        catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    }
    saveData(context, data, callback) {
        console.warn(StateApiDreprecatedMessage);
        var list = [];
        function addWrite(field, botData, url) {
            var hashKey = field + 'Hash';
            var hash = JSON.stringify(botData);
            if (!data[hashKey] || hash !== data[hashKey]) {
                data[hashKey] = hash;
                list.push({ botData: botData, url: url, hash: hash });
            }
        }
        try {
            var root = this.getStoragePath(context.address);
            if (context.userId) {
                if (context.persistUserData) {
                    addWrite('userData', data.userData || {}, root + '/users/' + encodeURIComponent(context.userId));
                }
                if (context.conversationId) {
                    var url = root + '/conversations/' + encodeURIComponent(context.conversationId) +
                        '/users/' + encodeURIComponent(context.userId);
                    addWrite('privateConversationData', data.privateConversationData || {}, url);
                }
            }
            if (context.persistConversationData && context.conversationId) {
                addWrite('conversationData', data.conversationData || {}, root + '/conversations/' + encodeURIComponent(context.conversationId));
            }
            async.each(list, (entry, cb) => {
                if (this.settings.gzipData) {
                    zlib.gzip(entry.hash, (err, result) => {
                        if (!err && result.length > MAX_DATA_LENGTH) {
                            err = new Error("Data of " + result.length + " bytes gzipped exceeds the " + MAX_DATA_LENGTH + " byte limit. Can't post to: " + entry.url);
                            err.code = consts.Errors.EMSGSIZE;
                        }
                        if (!err) {
                            var options = {
                                method: 'POST',
                                url: entry.url,
                                body: { eTag: '*', data: result.toString('base64') },
                                json: true
                            };
                            this.authenticatedRequest(options, (err, response, body) => {
                                cb(err);
                            });
                        }
                        else {
                            cb(err);
                        }
                    });
                }
                else if (entry.hash.length < MAX_DATA_LENGTH) {
                    var options = {
                        method: 'POST',
                        url: entry.url,
                        body: { eTag: '*', data: entry.botData },
                        json: true
                    };
                    this.authenticatedRequest(options, (err, response, body) => {
                        cb(err);
                    });
                }
                else {
                    var err = new Error("Data of " + entry.hash.length + " bytes exceeds the " + MAX_DATA_LENGTH + " byte limit. Consider setting connectors gzipData option. Can't post to: " + entry.url);
                    err.code = consts.Errors.EMSGSIZE;
                    cb(err);
                }
            }, (err) => {
                if (callback) {
                    if (!err) {
                        callback(null);
                    }
                    else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m));
                    }
                }
            });
        }
        catch (e) {
            if (callback) {
                var err = e instanceof Error ? e : new Error(e.toString());
                err.code = consts.Errors.EBADMSG;
                callback(err);
            }
        }
    }
    onDispatchEvents(events, callback) {
        if (events && events.length > 0) {
            if (this.isInvoke(events[0])) {
                this.onInvokeHandler(events[0], callback);
            }
            else {
                this.onEventHandler(events);
                callback(null, null, 202);
            }
        }
    }
    dispatch(msg, res, next) {
        try {
            this.prepIncomingMessage(msg);
            logger.info(msg, 'ChatConnector: message received.');
            this.onDispatchEvents([msg], (err, body, status) => {
                if (err) {
                    res.status(500);
                    res.end();
                    next();
                    logger.error('ChatConnector: error dispatching event(s) - ', err.message || '');
                }
                else if (body) {
                    res.send(status || 200, body);
                    res.end();
                    next();
                }
                else {
                    res.status(status || 200);
                    res.end();
                    next();
                }
            });
        }
        catch (e) {
            console.error(e instanceof Error ? e.stack : e.toString());
            res.status(500);
            res.end();
            next();
        }
    }
    isInvoke(event) {
        return (event && event.type && event.type.toLowerCase() == consts.invokeType);
    }
    postMessage(msg, lastMsg, cb, method = 'POST') {
        logger.info(address, 'ChatConnector: sending message.');
        this.prepOutgoingMessage(msg);
        var address = msg.address;
        msg['from'] = address.bot;
        msg['recipient'] = address.user;
        delete msg.address;
        if (msg.type === 'message' && !msg.inputHint) {
            msg.inputHint = lastMsg ? 'acceptingInput' : 'ignoringInput';
        }
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }
        var options = {
            method: method,
            url: urlJoin(address.serviceUrl, path),
            body: msg,
            json: true
        };
        this.authenticatedRequest(options, (err, response, body) => {
            if (!err) {
                if (body && body.id) {
                    let newAddress = utils.clone(address);
                    newAddress.id = body.id;
                    cb(null, newAddress);
                }
                else {
                    cb(null, address);
                }
            }
            else {
                cb(err, null);
            }
        });
    }
    authenticatedRequest(options, callback, refresh = false) {
        if (refresh) {
            this.accessToken = null;
        }
        this.addUserAgent(options);
        this.addAccessToken(options, (err) => {
            if (!err) {
                request(options, (err, response, body) => {
                    if (!err) {
                        switch (response.statusCode) {
                            case 401:
                            case 403:
                                if (!refresh && this.settings.appId && this.settings.appPassword) {
                                    this.authenticatedRequest(options, callback, true);
                                }
                                else {
                                    callback(null, response, body);
                                }
                                break;
                            default:
                                if (response.statusCode < 400) {
                                    callback(null, response, body);
                                }
                                else {
                                    var txt = options.method + " to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                                    callback(new Error(txt), response, null);
                                }
                                break;
                        }
                    }
                    else {
                        callback(err, null, null);
                    }
                });
            }
            else {
                callback(err, null, null);
            }
        });
    }
    tokenExpired() {
        return Date.now() >= this.accessTokenExpires;
    }
    tokenHalfWayExpired(secondstoHalfWayExpire = 1800, secondsToExpire = 300) {
        var timeToExpiration = (this.accessTokenExpires - Date.now()) / 1000;
        return timeToExpiration < secondstoHalfWayExpire
            && timeToExpiration > secondsToExpire;
    }
    refreshAccessToken(cb) {
        if (!this.refreshingToken) {
            this.refreshingToken = new Promise((resolve, reject) => {
                var opt = {
                    method: 'POST',
                    url: this.settings.endpoint.refreshEndpoint,
                    form: {
                        grant_type: 'client_credentials',
                        client_id: this.settings.appId,
                        client_secret: this.settings.appPassword,
                        scope: this.settings.endpoint.refreshScope
                    }
                };
                this.addUserAgent(opt);
                request(opt, (err, response, body) => {
                    this.refreshingToken = undefined;
                    if (!err) {
                        if (body && response.statusCode < 300) {
                            var oauthResponse = JSON.parse(body);
                            this.accessToken = oauthResponse.access_token;
                            this.accessTokenExpires = new Date().getTime() + ((oauthResponse.expires_in - 300) * 1000);
                            resolve(this.accessToken);
                        }
                        else {
                            reject(new Error('Refresh access token failed with status code: ' + response.statusCode));
                        }
                    }
                    else {
                        reject(err);
                    }
                });
            }).catch((err) => {
                this.refreshingToken = undefined;
                throw err;
            });
        }
        this.refreshingToken.then((token) => cb(null, token), (err) => cb(err, null));
    }
    getAccessToken(cb) {
        if (this.accessToken == null || this.tokenExpired()) {
            this.refreshAccessToken((err, token) => {
                cb(err, this.accessToken);
            });
        }
        else if (this.tokenHalfWayExpired()) {
            var oldToken = this.accessToken;
            this.refreshAccessToken((err, token) => {
                if (!err)
                    cb(null, this.accessToken);
                else
                    cb(null, oldToken);
            });
        }
        else
            cb(null, this.accessToken);
    }
    addUserAgent(options) {
        if (!options.headers) {
            options.headers = {};
        }
        options.headers['User-Agent'] = USER_AGENT;
    }
    addAccessToken(options, cb) {
        if (this.settings.appId && this.settings.appPassword) {
            this.getAccessToken((err, token) => {
                if (!err && token) {
                    if (!options.headers) {
                        options.headers = {};
                    }
                    options.headers['Authorization'] = 'Bearer ' + token;
                    cb(null);
                }
                else {
                    cb(err);
                }
            });
        }
        else {
            cb(null);
        }
    }
    getStoragePath(address) {
        var path;
        switch (address.channelId) {
            case 'emulator':
                if (address.serviceUrl) {
                    path = address.serviceUrl;
                }
                else {
                    throw new Error('ChatConnector.getStoragePath() missing address.serviceUrl.');
                }
                break;
            default:
                path = this.settings.endpoint.stateEndpoint;
                break;
        }
        return path + '/v3/botstate/' + encodeURIComponent(address.channelId);
    }
    prepIncomingMessage(msg) {
        utils.moveFieldsTo(msg, msg, {
            'locale': 'textLocale',
            'channelData': 'sourceEvent'
        });
        msg.text = msg.text || '';
        msg.attachments = msg.attachments || [];
        msg.entities = msg.entities || [];
        var address = {};
        utils.moveFieldsTo(msg, address, toAddress);
        msg.address = address;
        msg.source = address.channelId;
        if (msg.source == 'facebook' && msg.sourceEvent && msg.sourceEvent.message && msg.sourceEvent.message.quick_reply) {
            msg.text = msg.sourceEvent.message.quick_reply.payload;
        }
    }
    prepOutgoingMessage(msg) {
        if (msg.attachments) {
            var attachments = [];
            for (var i = 0; i < msg.attachments.length; i++) {
                var a = msg.attachments[i];
                switch (a.contentType) {
                    case 'application/vnd.microsoft.keyboard':
                        if (msg.address.channelId == 'facebook') {
                            msg.sourceEvent = { quick_replies: [] };
                            a.content.buttons.forEach((action) => {
                                switch (action.type) {
                                    case 'imBack':
                                    case 'postBack':
                                        msg.sourceEvent.quick_replies.push({
                                            content_type: 'text',
                                            title: action.title,
                                            payload: action.value
                                        });
                                        break;
                                    default:
                                        logger.warn(msg, "Invalid keyboard '%s' button sent to facebook.", action.type);
                                        break;
                                }
                            });
                        }
                        else {
                            a.contentType = 'application/vnd.microsoft.card.hero';
                            attachments.push(a);
                        }
                        break;
                    default:
                        attachments.push(a);
                        break;
                }
            }
            msg.attachments = attachments;
        }
        utils.moveFieldsTo(msg, msg, {
            'textLocale': 'locale',
            'sourceEvent': 'channelData'
        });
        delete msg.agent;
        delete msg.source;
        if (!msg.localTimestamp) {
            msg.localTimestamp = new Date().toISOString();
        }
    }
}
exports.ChatConnector = ChatConnector;
var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'conversation': 'conversation',
    'recipient': 'bot',
    'serviceUrl': 'serviceUrl'
};
