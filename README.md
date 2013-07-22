
The MIT License (MIT)

Copyright (c) 2013 Bryan Robert Holmes

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

        
          

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
