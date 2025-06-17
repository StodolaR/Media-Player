# Media-Player
Audio-video přehrávač založený na MediaElement s podporou obrázků pomocí Image a playlistů uložených v .xml
## Screenshots
![Obrázek](https://github.com/StodolaR/Media-Player/blob/master/Screenshots/Obrazek.png)
![Playlist obrázků](https://github.com/StodolaR/Media-Player/blob/master/Screenshots/ObrazkovyPlaylist.png)
![Audio playlist](https://github.com/StodolaR/Media-Player/blob/master/Screenshots/AudioPlaylist.png)
![Slideshow spolu s přehráváním audia](https://github.com/StodolaR/Media-Player/blob/master/Screenshots/SlideshowSAudiem.png)
![Video](https://github.com/StodolaR/Media-Player/blob/master/Screenshots/Video.png)
## Funkce MediaPlayeru
U MediaPlayeru lze přepínat do dvou módů - pro přehrávání videa nebo pro přehrávání audia spolu se zobrazením obrázků (lze i samostatně audio či obrázek).

V režimu videa lze spustit buď jedno video nebo vytvořit playlist s několika videy spouštěnými postupně.
Při spuštění videa lze toto pozastavit (Pause), posouvat děj přetažením či kliknutím na posuvník, obdobně měnit hlasitost.
Panel zobrazuje průběh přehrávání a název souboru. V případě spuštění playlistu lze navíc přecházet mezi položkami pomocí tlačítek vpřed a vzad.

Režim audia má stejné funkce jako režim videa, ale lze navíc při přehrávání zobrazit obrázek či playlist obrázků.
Pokud je spuštěn playlist obrázků s více obrázky, objeví se napravo seznam s miniaturami a na panelu informace o jejich počtu a nastavení intervalu slideshow.
Mezi obrázky lze volit kliknutím na miniaturu vpravo, nebo spustit postupné přehrávání pomocí tlačítka slideshow.
## Funkce Playlist okna
Podle zvoleného režimu se spustí playlisty pro video či audio/obrázky - mezi audiem a obrázky se dále přepíná pomocí tlačítka. 
Vedle titulku lze vidět, který typ playlistů je právě zobrazen.

Levou část okna tvoří seznam vybraných playlistů. Lze zde vytvořit nový playlist či přejmenovat již vytvořený (program hlídá, aby se názvy neopakovaly).
Dále zde lze vybraný playlist smazat a ze seznamu vybrat playlist pro editaci či spuštění.

Pro editaci playlistu slouží pravá část okna. Tlačítka napravo slouží k:
  - posunu položky o pozici výše
  - přidání celé složky se soubory do playlistu
  - přidání jen jednoho souboru do playlistu
  - smazání položky playlistu
  - posunu položky o pozici níže

Výběrem pod tlačítky lze určit, zda přidávat položky či složky na konec seznamu či před zvolenou položku.
Při přidání celé složky je označen její začátek a konec a při výběru jejího označení lze manipulovat (posouvat nahoru či dolů, smazat) celou vybranou složku.
Lze i vysouvat položky ze složky či do ní nasouvat cizí nebo vložit jednu složku do druhé.

Vybraný playlist lze pak spustit tlačítky nad pravým seznamem buď celý nebo lze načíst pro přehrávání jen položky od vybrané dále.
## Další funkce aplikace
Program hlídá, jestli soubor položky skutečně existuje a jestli není poškozen, případně oznámí první chybnou položku, 
všechny chybné v řadě smaže z aktuálního přehrávání (ne z originálního playlistu) a přehrává dál od nejbližší funkční položky či oznámí dosažení konce playlistu.

Tlačítkem vpravo dole lze ovládací panel schovat či opět zobrazit. Tlačítka jsou popsána tooltipy pro lepší orientaci.

A jako bonus - obdelníčky kolem displeje názvů souborů slouží k úpravě vzhledu. 
Ty nalevo od displeje upravují odstín barev pozadí panelu a okna playlistu, ty napravo odstín barvy tlačítek. 



