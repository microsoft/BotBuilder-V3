/* ------------------------------------------------------------------------------------------
*   LUIS Dialog
*   This file contains a dialog for use with Language Understanding Intelligent Service (LUIS)
*   You can find out more information at https://luis.ai
*
*   To use this dialog:
*   1. Create a model in LUIS
*   2. Update the LUIS_MODEL_URL process variable, through .env or directly, with the URL
*       you obtained from step one
*   3. Update the code below to prompt the user for missing entities
------------------------------------------------------------------------------------------ */

const builder = require('botbuilder');

module.exports = {
    id: 'luis',
    name: 'luis',
    waterfall: [
        (session, args, next) => {
            const entity = builder.EntityRecognizer.findEntity(args.entities, 'entity');
            if (entity)
                next({ response: entity.entity });
            else
                builder.Prompts.text(session, 'Please provide entityName');
        },
        (session, results, next) => {
            session.endConversation(`You said ${results.response}`);
        }
    ]
};