---
layout: page
title: Bot review guidelines
permalink: /directory/review-guidelines/
weight: 720
parent1: Bot Directory
---

<style>
      ol.list-counter {
        padding:1ex 0;
        counter-reset: category;
        list-style-position: inside;
      }
      ol.list-counter > li {
        margin-top: 1rem;
        margin-bottom: 1rem;
        counter-increment: category;
      }
      
      ol.list-counter ol {
        padding:1ex 0;
        list-style-type: decimal;
        list-style-position: inside;
        margin-left: 2em;
        margin-bottom: 1em;        
      }
      ol.list-counter ol li {
        position: relative;        
        display: block;
        counter-increment: item;
        padding-left: 1ex;
        padding-bottom: 1ex;
      }
      
      ol.list-counter ol li:firt-child {
        position: relative;        
        display: block;
        counter-reset: item;
      }
      
      ol.list-counter  ol  li:before {
        position: absolute;        
        content: counter(category)'.'counter(item);
        left: -2em;
        text-align: right;        
      }

    </style>

We welcome you and thank you for investing your talents and time in building Bots using Microsoft's Bot Framework. Microsoft will review your bot submission to make sure it meets certain minimum requirements before it is publicly available on the Bot Directory. Following are some criteria we use to evaluate your bot before publication to the Bot Directory:  

<section>
    <ol class="list-counter">
<li>Your bot must do something meaningful that adds value to the user (e.g., respond to basic commands, have at least some level of communication etc.)</li>

<li>The bot profile image, name, keywords and description must NOT:</li>
<ol>
<li>Be offensive or explicit;</li>
<li>Include third party trademarks, service marks or logos;</li>
<li>Impersonate or imply endorsement by a third party; </li>
<li>Use names unrelated to the bot;</li>
<li>Use Microsoft logos, trademarks or service marks unless you have permission from Microsoft;</li>
<li>Be too long or verbose. The description should be 8-10 words.</li>
</ol>
<li>Your Terms of Service and Privacy Policy links are required for submission and publication to the Bot Directory. In addition, you will need to ensure that you follow the privacy notice requirements as communicated in the Microsoft Bot Framework Code of Conduct referenced here: <a href="https://aka.ms/bf-conduct">https://aka.ms/bf-conduct</a></li>

<li>The bot must operate as described in its bot description, profile, terms of use and privacy policy. You must notify Microsoft in advance if you make any material changes to your bot.  Microsoft has the right, in its sole discretion, to remove your bot from the Bot Directory without notice. </li>

<li>Your bot must operate in accordance with the requirements set forth in the Microsoft Online Services Agreement and Microsoft Bot Framework Code of Conduct.</li>

<li>Changes made to your bot's registration may require your bot to be re-reviewed to ensure that it continues to meet the requirements stated here.</li>

<li>Although Microsoft will review your bot to confirm it meets certain minimum requirements prior to publication on the Bot Directory, you are solely responsible for: (1) your bot; (2) its content and actions; (3) compliance with all applicable laws; (4) compliance with any third party terms and conditions; and (5) compliance with the Microsoft Online Services Agreement, Privacy Statement and Microsoft Bot Framework Code of Conduct.  Microsoft's review and publication of your bot to the Bot Directory is not an endorsement of your bot.  </lI>
 
<li>Keep your Microsoft Account email active as we will use that for all bot-related communication with you.</li>
    </ol>
  </section>