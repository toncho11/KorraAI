# KorraAI

KorraAI is a bot framework that allows you to create your own bot, also called Embodied Conversational Agent ([ECA](https://en.wikipedia.org/wiki/Embodied_agent)). You can learn more about the design principles in [Philosophy](../../wiki/Philosophy) and the technical aspects in [TechnicalSpec](../../wiki/TechnicalSpec).

*Yes, you can encode a girlfriend! :)*

Advantages of using KorraAI:

* Being proacvtive by design, the bot will continuesly initiate interaction in distribution controlled manner
* Comes with Probabilisitic Programming library that allows for distributions and Bayesian networks encoding of concepts from real-life
* Behavior change can be encoded based on elpased time or external factors, user responses, etc.
* Has a quality 3D model, voice synthesis, lips sync, bot movement animation, eye movements and blinks, voice annotation (SSML)
* The result is a Unity 3D application that can be executed on many platforms
* It is plugin based, so some of the techical aspects are hidden for the sake of simplicity, so you can focus on modelling

In order to start coding for KorraAI you need to go through these pages:

*  [Tutorial](../../wiki/Tutorial)
*  [KorraAIModel class](../../wiki/KorraAIModel-class)
*  [Add a concept](../../wiki/Add-a-concept)
*  [Probabilistic Examples](../../wiki/Probabilistic-Examples)

And also:
*  [Building a response](../../wiki/Building-a-response)
*  [Distribution over the items inside a category](../../wiki/Distribution-inside-a-category)
*  [Adjust when a category runs out of items](../../wiki/Planning-and-running-out-of-items)

**Example 1:**
The probability of saying a joke can depend on a base joke probability (that describes the overall character of the bot) and the mood of the user. This is a simple Bayes network of three variables. For example a bad mood can indicate that it is better that the bot says more jokes (to improve the mood the interlocutor). So if the user has indicated a bad mood we can infer a higher chance of saying a joke, for example 10 jokes per hour in comparison to 7 jokes per hour, if he declared at some point that he was in a good mood.

**Example 2:**
Time is also encoded as distributions and not fixed intervals. For example we can reduce the time between interactions, but not by a fixed amount. We do that by changing the parameters of the distribution (ex. Normal(m,n) distribution) so that the pauses between interactions are centered for example around 2.1 seconds and not 3.1 seconds. This will make the bot more interactive and choosing the right value depends on the psychological effect you would like to encode.

**Application:**
KorraAI bots can be used in video games and for the conduct of psychological experiments. For example one can try to encode specific behavior and then validate: how successful was the encoding or what are the effects of this behavior on human subjects.

Possible commercial applications:

- Sales agent
- TV presenter
- Entertainer
- Companion


[Installation](../../wiki/Installation)
