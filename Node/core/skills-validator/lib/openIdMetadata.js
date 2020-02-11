"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var fetch = require('node-fetch');
var getPem = require('rsa-pem-from-mod-exp');
var base64url = require('base64url');
var OpenIdMetadata = (function () {
    function OpenIdMetadata(url) {
        this.lastUpdated = 0;
        this.url = url;
    }
    OpenIdMetadata.prototype.getKey = function (keyId) {
        return __awaiter(this, void 0, void 0, function () {
            var key, err_1, key;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!(this.lastUpdated < (Date.now() - 1000 * 60 * 60 * 24 * 5))) return [3, 5];
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4, this.refreshCache()];
                    case 2:
                        _a.sent();
                        key = this.findKey(keyId);
                        return [2, key];
                    case 3:
                        err_1 = _a.sent();
                        throw err_1;
                    case 4: return [3, 6];
                    case 5:
                        key = this.findKey(keyId);
                        return [2, key];
                    case 6: return [2];
                }
            });
        });
    };
    OpenIdMetadata.prototype.refreshCache = function () {
        return __awaiter(this, void 0, void 0, function () {
            var res, openIdConfig, getKeyResponse, _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0: return [4, fetch(this.url)];
                    case 1:
                        res = _b.sent();
                        if (!res.ok) return [3, 7];
                        return [4, res.json()];
                    case 2:
                        openIdConfig = _b.sent();
                        return [4, fetch(openIdConfig.jwks_uri)];
                    case 3:
                        getKeyResponse = _b.sent();
                        if (!getKeyResponse.ok) return [3, 5];
                        this.lastUpdated = new Date().getTime();
                        _a = this;
                        return [4, getKeyResponse.json()];
                    case 4:
                        _a.keys = (_b.sent()).keys;
                        return [3, 6];
                    case 5: throw new Error("Failed to load Keys: " + getKeyResponse.status);
                    case 6: return [3, 8];
                    case 7: throw new Error("Failed to load openID config: " + res.status);
                    case 8: return [2];
                }
            });
        });
    };
    OpenIdMetadata.prototype.findKey = function (keyId) {
        if (!this.keys) {
            return null;
        }
        for (var _i = 0, _a = this.keys; _i < _a.length; _i++) {
            var key = _a[_i];
            if (key.kid === keyId) {
                if (!key.n || !key.e) {
                    return null;
                }
                var modulus = base64url.toBase64(key.n);
                var exponent = key.e;
                return { key: getPem(modulus, exponent), endorsements: key.endorsements };
            }
        }
        return null;
    };
    return OpenIdMetadata;
}());
exports.OpenIdMetadata = OpenIdMetadata;
