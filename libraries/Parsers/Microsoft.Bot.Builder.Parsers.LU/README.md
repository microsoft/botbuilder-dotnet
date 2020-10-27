This package is intended for Microsoft use only. It is not designed to be consumed as an independent package.

# LU Parser ***_[PREVIEW]_***

An .lu file contains Markdown-like, simple text based definitions for Language Understanding Concepts 
LU parser returns a LuResource object which is the abstraction of Language Understanding elements (Intents, Entities, Utterances...)

```c#
            var luContent = "> This is a comment and will be ignored.\n # Test\n- greeting";
            var result = LuParser.Parse(luContent);  // this returns LuResource object
```

This is a sample of the serialized LuResource.

```javascript
{
   "Sections":[
      {
         "Errors":[
            
         ],
         "SectionType":"modelInfoSection",
         "Id":"modelInfoSection_> !# @app.name = all345",
         "Body":"",
         "ModelInfo":"> !# @app.name = all345",
         "Range":{
            "Start":{
               "Line":1,
               "Character":0
            },
            "End":{
               "Line":1,
               "Character":23
            }
         }
      },
      {
         "Errors":[
            
         ],
         "SectionType":"simpleIntentSection",
         "Id":"simpleIntentSection_test",
         "Body":"- greeting",
         "UtteranceAndEntitiesMap":[
            {
               "utterance":"greeting",
               "entities":[
                  
               ],
               "errorMsgs":[
                  
               ],
               "contextText":"- greeting",
               "range":{
                  "Start":{
                     "Line":8,
                     "Character":0
                  },
                  "End":{
                     "Line":8,
                     "Character":10
                  }
               }
            }
         ],
         "Entities":[
            
         ],
         "Name":"test",
         "IntentNameLine":"# test",
         "Range":{
            "Start":{
               "Line":7,
               "Character":0
            },
            "End":{
               "Line":8,
               "Character":10
            }
         }
      }
   ],
   "Content":"> !# @app.name = all345\n > # test \n- greeting",
   "Errors":[
      
   ]
}
```

# .lu file format

This content has moved and available [here](https://aka.ms/lu-file-format)
