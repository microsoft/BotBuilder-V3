import builder = require('botbuilder');
import alarms = require('../AlarmScheduler');
import consts = require('../consts');

var isCreated = false;
var scheduler: alarms.IAlarmScheduler;

/**
 * The dialogs Unique ID.
 */
export function getId() {
    return 'main';
}

/**
 * Creates the dialog.
 */
export function create(alarmScheduler: alarms.IAlarmScheduler) {
    if (!isCreated) {
        isCreated = true;
        scheduler = alarmScheduler;

        // Create dialog
        builder.LuisDialog.create(getId(), 'https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=')
            .on('builtin.intent.alarm.set_alarm', onSetAlarmStart, onSetAlarmContinue)
            .on('builtin.intent.alarm.alarm_other', onChangeAlarmStart, onChangeAlarmContinue)
            .on('builtin.intent.alarm.delete_alarm', onDeleteAlarmStart, onDeleteAlarmContinue)
            .on('builtin.intent.alarm.find_alarm', onFindAlarmStart, onFindAlarmContinue)
            .on('builtin.intent.alarm.snooze', onSnoozeStart, onSnoozeContinue)
            .on('builtin.intent.alarm.time_remaining', onTimeRemainingStart, onTimeRemainingContinue)
            .on('builtin.intent.alarm.turn_off_alarm', onTurnOffAlarmStart, onTurnOffAlarmContinue)
            .onDefault((session, entities, intents: builder.IIntent[]) => {
                sendEntities(session, 'Unknown intent "' + intents[0].intent + '" with entities:\n\n%s', entities);
            });
    }
}

//=============================================================================
// Set Alarm. Sample Utterances:
//
//      turn on my wake up alarm
//      can you set an alarm for 12 called take antibiotics?
//
//=============================================================================

function onSetAlarmStart(session: builder.ISession, entities: builder.IEntity[]) {
    // Parse entities
    var title = builder.LuisEntityResolver.findEntity(entities, consts.entities.title);
    var date = builder.LuisEntityResolver.resolveDate(entities);

    // Save initial alarm data
    session.data.setDialogState('alarm', {
        title: title ? title.entity : '',
        timestamp: date ? date.getTime() : null,
        timezone: session.data.getUserData<string>(consts.user.properties.timezone, '')
    });

    // Fill in missing data
    fillAlarmData(session, null, onAlarmDataCompleted);
};

function onSetAlarmContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
    // Fill in result
    fillAlarmData(session, result, onAlarmDataCompleted);
}

function fillAlarmData(session: builder.ISession, promptResult: builder.IPromptResult<any>, onCompleted: (session, data) => void) {
    var data = session.data.getDialogState<alarms.IAlarmData>('alarm');

    // Fill in data if we got a prompt result
    if (promptResult) {
        var field = session.data.getDialogState<string>('field');
        switch (field) {
            case 'title':
                data.title = promptResult.response || '';
                break;
            case 'timestamp':
                data.timestamp = promptResult.response ?
                    (<builder.IChronoDuration>promptResult.response).resolution.start.getTime() :
                    new Date().getTime();
                break;
            case 'timezone':
                data.timezone = promptResult.response || 'PST';
                session.data.setUserData(consts.user.properties.timezone, data.timezone);
                break;
        }
        session.data.setDialogState('alarm', data);
    }

    // Validate completion of data. Prompt for missing fields.
    if (!data.title || data.title.trim().length == 0) {
        session.data.setDialogState('field', 'title');
        builder.Prompts.text(session, 'What would you like to call your alarm?');
    } else if (!data.timestamp) {
        session.data.setDialogState('field', 'timestamp');
        builder.Prompts.time(session, 'When would you like me to notify you?');
    } else if (!data.timezone || data.timezone.length == 0) {
        session.data.setDialogState('field', 'timezone');
        builder.Prompts.choice(session, 'What is your current timezone?', ['PST', 'MST', 'CST', 'EST']);
    } else {
        onCompleted(session, data);
    }
}

function onAlarmDataCompleted(session: builder.ISession, data: alarms.IAlarmData) {
    var date = new Date(data.timestamp);
    var isAM = date.getHours() < 12;
    session.send('Creating alarm named "%s" for %d/%d/%d %d:%02d%s %s',
        data.title,
        date.getMonth() + 1, date.getDate(), date.getFullYear(),
        isAM ? date.getHours() : date.getHours() - 12, date.getMinutes(), isAM ? 'am' : 'pm',
        data.timezone);
}

//=============================================================================
// Change Alarm. Sample Utterances:
// 
//      update my 7:30 alarm to be eight o'clock
//      change my alarm from 8am to 9am
//
//=============================================================================

function onChangeAlarmStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onChangeAlarmStart entities:\n\n%s', entities);
}

function onChangeAlarmContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// Delete Alarm. Sample Utterances:
// 
//      delete an alarm
//      delete my alarm "wake up"
//
//=============================================================================

function onDeleteAlarmStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onDeleteAlarmStart entities:\n\n%s', entities);
}

function onDeleteAlarmContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// Find Alarm. Sample Utterances:
// 
//      what time is my wake-up alarm set for?
//      is my wake-up alarm on?
//
//=============================================================================

function onFindAlarmStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onFindAlarmStart entities:\n\n%s', entities);
}

function onFindAlarmContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// Snooze. Sample Utterances:
// 
//      snooze alarm for 5 minutes
//      snooze alarm
//
//=============================================================================

function onSnoozeStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onSnoozeStart entities:\n\n%s', entities);
}

function onSnoozeContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// TimeRemaining. Sample Utterances:
// 
//      how much longer do i have until "wake-up"?
//      how much time until my next alarm?
//
//=============================================================================

function onTimeRemainingStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onTimeRemainingStart entities:\n\n%s', entities);
}

function onTimeRemainingContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// TurnOff Alarm. Sample Utterances:
// 
//      turn off my 7am alarm
//      turn off my wake up alarm
//
//=============================================================================

function onTurnOffAlarmStart(session: builder.ISession, entities: builder.IEntity[]) {
    sendEntities(session, 'onTurnOffAlarmStart entities:\n\n%s', entities);
}

function onTurnOffAlarmContinue(session: builder.ISession, result: builder.IPromptResult<any>) {
}

//=============================================================================
// Helper Functions
//=============================================================================


function sendEntities(session: builder.ISession, tmpl: string, entities: builder.IEntity[]) {
    var list = '';
    for (var i = 0; i < entities.length; i++) {
        var entity = entities[i];
        list += '- ' + entity.type + ': ' + entity.entity + '\n';
    }
    session.send(tmpl, list);
}