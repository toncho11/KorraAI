# KorraAI

Is a framework that allows you to create yout own bot, also called Embodied Conversational Agent (ECA). It uses distributions and probabilisitic programming to encode behavior and unexpected flow of events. The currently enoded bot is called Joi, a female one. Probabilsitic programming is a way to encode Bayes networks. Otherwise said it is a way to manage uncertain concepts. 

*Yes ... you can encode a girlfriend ...*

**Example 1:**
The probability of saying a joke can depend on a base joke probability (that describes the overall character of the bot) and the moood of the user. This is a simple Bayes network of three variables. For example a bad mood can indicate that it is better that the bot says more jokes (to improve the mood the interlocutor). So if the user has indicated a bad mood we can infer a higher chance of saying a joke, for example 10 jokes per hour in comparison to 7 jokes per hour, if he declared at some point that he was in a good mood. The "mood" on the other hand can be both: a directly answered question or implicitly inferred based on other concepts modeled as probabilitic variables in a Bayes network.

**Example 2:**
Time is also encoded as distributions and not fixed intervals. For example we can reduce the time between interactions, but not by a fixed amount. We do that by changing the parameters of the distribution (ex. Normal(m,n) distribution) so that the pauses between interactions are centered for example around 2.1 seconds and not 3.1 seconds. This will make the bot more interactive and choose the right value depends on the psychological effect you would like to encode.

It is recommended that you jump to the [Tutorial](../../wiki/Tutorial)

**Cognitive and physical cabilties:**
The bot is programmed in Unity and comes with a quality 3D model, voice synthesis, lips sync, simple bot movement, eye movements and blinks, voice annotation, at least 6 cloth outfits and other that make it look human-like. No voice recognition is currently included. Interaction with the bot is done with mouse and keyboard.

See more in [Examples](../../wiki/Examples). Text is also submitted to randomization, so that not the same phrase is used each time.

**Application:**
- TV presenter
- Entertainer
- Sales agent

[Installation](../../wiki/Installation)
