Die Kalenderapp hat links einen Kalender, dieser Dient der Navigation.
Wenn ein Tag per Linksklick angeklickt wird und anschließend das Uhrensymbol geklickt wird, dann kann man einen Termin anlegen.

Wenn der Termin angelegt ist, dann sieht man diesen rechts sowohl in einer Gridview als auch darunter in einer Monatskalenderdarstellung.

Wenn man einen Termin angelegt hat, dann kann man dessen Details ansehen, indem man auf den Termin in der GridView klickt, mit x kann man diesen löschen.
Wenn der Termin angeklickt wird, dann hat man seinerseits eine GridView aller Kontakte. Wenn ein Kontakt geklickt wird, dann kann man diesen dem 
Termin hinzufügen, die Verknüpfung wird dann durch farbige Unterlegung angezeigt.

Die Kontakt-Ansicht erreicht man, indem man auf den Männchen-Button klickt. Dort kann man Kontakte anlegen, bearbeiten, löschen.

Mit dem ICS-Button kann man eine Kalenderdatei im Google-Calendar-Format einlesen.

Das Wolkensymbol kann geklicjt werden, wenn man in das Textfeld neben dem Wolkensymbol einen Ort eingibt - man erhält dann einige Wetterdaten.

Rechts neben dem ICS-Button ist ein Chatbutton, der Gruppenchat funktioniert aber nur, wenn der Server up ist... 

Ich habe eine sqlite-Datenbank namens cal.db mit folgendem Schema:

create table dates (id integer primary key autoincrement, text text, start text, end text, duration text, repeat text); 

create couples (id_date text, id_contact text, iscouple text);

create contacts (id integer primary key autoincrement, name text);

creat table language(lang TEXT);

create table exceptions(id_date TEXT, startexception TEXT, endexception TEXT);     