var MemoryBotStorage = (function () {
    function MemoryBotStorage() {
        this.userStore = {};
        this.conversationStore = {};
    }
    MemoryBotStorage.prototype.get = function (address, callback) {
        var data = {};
        if (address.userId) {
            if (this.userStore.hasOwnProperty(address.userId)) {
                data.userData = JSON.parse(this.userStore[address.userId]);
            }
            else {
                data.userData = null;
            }
            if (address.conversationId) {
                var key = address.userId + ':' + address.conversationId;
                if (this.conversationStore.hasOwnProperty(key)) {
                    data.conversationData = JSON.parse(this.conversationStore[key]);
                }
                else {
                    data.conversationData = null;
                }
            }
        }
        callback(null, data);
    };
    MemoryBotStorage.prototype.save = function (address, data, callback) {
        if (address.userId) {
            this.userStore[address.userId] = JSON.stringify(data.userData || {});
            if (address.conversationId) {
                var key = address.userId + ':' + address.conversationId;
                this.conversationStore[key] = JSON.stringify(data.conversationData || {});
            }
        }
        callback(null);
    };
    MemoryBotStorage.prototype.delete = function (address) {
        if (address.userId && this.userStore.hasOwnProperty(address.userId)) {
            if (address.conversationId) {
                if (this.conversationStore.hasOwnProperty(address.conversationId)) {
                    delete this.conversationStore[address.conversationId];
                }
            }
            else {
                delete this.userStore[address.userId];
                for (var key in this.conversationStore) {
                    if (key.indexOf(address.userId + ':') == 0) {
                        delete this.conversationStore[key];
                    }
                }
            }
        }
    };
    return MemoryBotStorage;
})();
exports.MemoryBotStorage = MemoryBotStorage;
