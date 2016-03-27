---
layout: page
title: How to use the QnA Maker
permalink: /qnamaker/howto/
weight: 1200
parent1: Sandbox
parent2: QnA Maker
---

###Go To [http://qnamaker.botframework.com/](http://qnamaker.botframework.com/ "QnA Maker")

The QnA Maker tool will attempt to extract questions and answers from a bunch of URLs and combine it with editorial content to create a knowledge base which can be published via an endpoint.

Currently the tool supports extraction from URLs which have explicit questions and answers, like FAQ pages. Examples:

-  [http://windows.microsoft.com/en-us/windows-10/upgrade-to-windows-10-faq](http://windows.microsoft.com/en-us/windows-10/upgrade-to-windows-10-faq)
-  [https://www.creditonebank.com/faqs.aspx](https://www.creditonebank.com/faqs.aspx)
-  [https://www.bankofamerica.com/credit-cards/account-access-faq.go](https://www.bankofamerica.com/credit-cards/account-access-faq.go)
-  [https://www.irs.gov/Retirement-Plans/Retirement-Plans-FAQs-regarding-Required-Minimum-Distributions](https://www.irs.gov/Retirement-Plans/Retirement-Plans-FAQs-regarding-Required-Minimum-Distributions)
-  [https://childsupport.oag.state.tx.us/wps/portal/csi/Faqs](https://childsupport.oag.state.tx.us/wps/portal/csi/Faqs)

## Step 1

Go to: [https://intercom-devportal-scratch.azurewebsites.net/default.aspx](https://intercom-devportal-scratch.azurewebsites.net/default.aspx), Click on the QnAWizard (at the bottom)
![System Overview of the Bot Framework](/images/qnamaker-howto-step1.png)
## Step 2

Sign in with your MSA (Outlook.com/Live.com) & click on " **Create New**"
![System Overview of the Bot Framework](/images/qnamaker-howto-step2.png)
## Step 3

Fill in the details & press Train. Give URLs to FAQ content of your domain. Example:

-  [https://www.creditonebank.com/faqs.aspx](https://www.creditonebank.com/faqs.aspx)
-  [http://windows.microsoft.com/en-us/windows-10/upgrade-to-windows-10-faq](http://windows.microsoft.com/en-us/windows-10/upgrade-to-windows-10-faq)

You can also add custom question and answers.

Then press **Train**
![System Overview of the Bot Framework](/images/qnamaker-howto-step3.png)
## Step 4

Once the content has been created, test your knowledge base with sample questions.
![System Overview of the Bot Framework](/images/qnamaker-howto-step4.png)
## Step 5

Once satisfied, publish the KB Service
![System Overview of the Bot Framework](/images/qnamaker-howto-step5.png)

Note down the service URL
![System Overview of the Bot Framework](/images/qnamaker-howto-step6.png)




# Using the QnA EndPoint in Bot Service

The QnA endpoint returns a JSON response for a question, with a response and a confidence score. You can use the confidence score to decide how to treat the response. Replace this with the service endpoint created in the above steps.

        ///<summary>
        
        /// QnA Service JSON response
        
        ///</summary>
        
        publicclassAnswerResponse
        
        {
        
            publicstring answer { get; set; }
        
            publicdouble score { get; set; }
        
        }
        
        ///<summary>
        
        /// Call the QnA service end-point wiht the user input to see if we have a response in our knowledge base.
        
        ///</summary>
        
        ///<param name="messageText"></param>
        
        ///<returns></returns>
        
        staticasyncTask<AnswerResponse> QnAService(string messageText)
        
        {

            AnswerResponse QnAReply = newAnswerResponse();
        
            //Paste the QnA endpoint below
        
            string QnAServiceURL = "http://knowledgebaseservice1.cloudapp.net/KBService.svc/GetAnswer?kbId=98ea048d9e1444f99fb78c624c1cee21&question=";
        
            try
        
            {
        
                using (var client = newHttpClient())
        
                {
        
                    client.DefaultRequestHeaders.Accept.Add(newMediaTypeWithQualityHeaderValue("text/plain"));
        
                    // HTTP GET with the question
        
                    var response = await client.GetAsync(QnAServiceURL + messageText);
        
                    if (response.IsSuccessStatusCode)
        
                    {
        
                        string QnAStringReply = await response.Content.ReadAsStringAsync();
        
                        QnAReply = JsonConvert.DeserializeObject<AnswerResponse>(Regex.Unescape(QnAStringReply));
        
                    }
        
                }
        
            }
        
            catch (HttpRequestException e)
        
            {
        
                // Something went wrong
        
                QnAReply.answer = "Something went wrong";
        
                QnAReply.score = 0;
        
            }
    
            return QnAReply;
        }
