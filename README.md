# CarMessenger

## Instrucțiuni de instalare și utilizare pentru Windows

  Instalează programul Visual Studio 2019 de pe siteul oficial. Selectează ‘ASP.NET and web development’ și începe instalarea. 
	
  
  După instalare, deschide Visual Studio, selectează ‘Open a project or solution’ și navighează până în fișierul cu aplicația CarMessenger pentru a selecta ‘CarMessenger.sln’ și apoi apăsați open sau deschideți în funcție de ce vă apare. Deoarece s-ar putea să nu aveți toate pachetele necesare instalate sau să le aveți cu versiunile greșite, deschideți consola pentru managementul pachetelor (Package Manager Console), prescurtat consola PM, și rulați comanda ‘Update-Package -Reinstall’. Pentru a deschide consola PM este nevoie sa mergeți la în bara cea mai de sus la View 🡪 Other Windows 🡪 Package Manager Console. Cel mai probabil vi se va spune că unele fișiere există deja și vă întreabă dacă să le înlocuiți. La acest tip de întrebare răspundeți cu nu și dacă aveți opțiunea nu la toate o puteți alege pentru a nu fi deranjați pe parcursul reinstalărilor. 
	
  
  Acum Visual Studio ar trebui să poată compila aplicația. După ce a fost compilată cu succes, puteți s-o rulați și este de preferat să selectați browserul Google Chrome. La prima rulare vă va apărea un avertisment în care veți fi întreabați dacă aveți încredere în certificatul emis de aplicație pentru a o rula pe calculator, cel mai bine este să răspundeți cu da și să bifați căsuța pentru a nu mai fi deranjați de acest mesaj. După aceea, acceptați instalarea certificatului.
	
  
  Aplicația ar trebui să pornească pe localhost în browser-ul selectat. Este bine să știți că în acest moment baza de date locală nu a fost creată și orice încercare de autentificare sau de înregistrare va eșua. Deschideți Solution Explorer-ul, iar dacă nu-l puteși vedea, îl puteți găsi în View 🡪 Solution Explorer. Click dreapta pe folderul App_Data, Add 🡪 New Item, apoi în noua fereastră apasați pe Installed în partea stângă și apoi selectați SQL Server Database. Scrieți în locul numelui curent ‘aspnet-CarMessenger.mdf’ și apoi dați click pe Add. 
  
  
  Deschidem din nou, daca nu este deja deschisă, consola PM. Executăm în consolă comanda ‘enable-migrations’, iar dacă vă spune că migrarile sunt deja activate, ignorați și mergeți mai departe. Executați comanda ‘add-migration 0_CreateAppDB’, după care comanda ‘update-database’.
Acum aplicația poate fi testată. Pentru a face aplicația publică în reteaua locală a serverului, trebuie să vă instalați pachetul Conveyor by Keyoti care va porni automat la pornirea serverului.
