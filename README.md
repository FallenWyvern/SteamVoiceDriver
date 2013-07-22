This is a Voice Recognition application for the Steam Digital Distribution network.
The primary function is to give complete, or as near as we can get, control over the 
Steam application as well as the applications launched in Steam.

This application first takes a JSON listing of all the games from Steam, then
loads up any overrides for Voice Keys, Game Names, Game specific voice keys
and Commands then loads these all as grammars for the voice recognition to
check.

There are two forms, the main form which has most of the logic already in it and
a secondary form for dictation. This is because using both dictation and enforced
grammars was very complicated and often ended up being more wrong than having two
separate instances. The two forms are great because it keeps these objects very
separate from each other.
