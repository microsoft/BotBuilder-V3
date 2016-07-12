exports.channels = {
    facebook: 'facebook',
    skype: 'skype',
    telegram: 'telegram',
    kik: 'kik',
    email: 'email',
    slack: 'slack',
    groupme: 'groupme',
    sms: 'sms',
    emulator: 'emulator'
};
function preferButtons(session, choiceCnt, rePrompt) {
    switch (getChannelId(session)) {
        case exports.channels.facebook:
        case exports.channels.skype:
            return (choiceCnt <= 3);
        case exports.channels.telegram:
        case exports.channels.kik:
        case exports.channels.emulator:
            return true;
        default:
            return false;
    }
}
exports.preferButtons = preferButtons;
function getChannelId(addressable) {
    var channelId;
    if (addressable) {
        if (addressable.hasOwnProperty('message')) {
            channelId = addressable.message.address.channelId;
        }
        else if (addressable.hasOwnProperty('address')) {
            channelId = addressable.address.channelId;
        }
        else if (addressable.hasOwnProperty('channelId')) {
            channelId = addressable.channelId;
        }
    }
    return channelId ? channelId.toLowerCase() : '';
}
exports.getChannelId = getChannelId;
