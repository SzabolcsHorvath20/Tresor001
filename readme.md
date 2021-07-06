Leírás

Végpontok:

http://localhost:24018/api/Product

GET request, visszaadja az összes terméket listázva.


http://localhost:24018/api/Product?id=Doll

GET request, a végpont visszaadja a kívánt termék adatait.

http://localhost:24018/api/Review?id=Doll&rows=All&category=Toys 
http://localhost:24018/api/Review?id=Doll&rows=5&category=Toys

A végpont GET request esetén visszaadja a kívánt termékhez tartozó véleményeket, ha biztosítjuk a megfelelő paramétereket.
A rows változó megadja, hogy hány véleményt szeretnénk lekérni.
A category változó az adatbázis optimálisabb működéséhez szükséges,
ugyanis a vélemények a termékkategóriák és azon belül termék azonosítók szerint vannak csoportosítva.
Ez nagyobb mennyiségű termék esetén jóval le tudja rövidíteni a keresési időt.
A limitált számú legfrissebb vélemények megtekintése rögzítésre kerül.
Ha a sorok száma helyett, az All szó kerül elküldésre, akkor az összes vélemény megjelenik.


http://localhost:24018/api/Review

Ezen a végponton POST request esetén véleményt tudunk rögzíteni.
A programban több ellenőrzési pont is van, ami meghatározza, hogy lehet-e rögzíteni véleményt.
A rögzítés csak akkor lehetséges, ha a termék létezik, a szövegezés kevesebb, mint 500 karakter,
a pontozás 1 és 5 között van, valamint, hogy történt-e már az adott felhasználó már látta-e a legutóbbi véleményeket.
Amikor egy új vélemény kerül be a rendszerbe, akkor a termék összesített pontszáma is frissítésre kerül.

Adatbázis:

Product Table			Itt tárolódnak a termékek.

String PartitionKey		A termék kategóriája
String RowKey			A termék azonosítója(név)
DateTime Timestamp		A termék felvételének ideje
String product_description	A termék leírása
Double product_rating		A termék összesített értékelése

Review Table			Itt tárolódnak a vélemények.

String PartitionKey		A termék neve, amiről az értékelés szól
String RowKey			A vélemény azonosítója(szám), ezt a program adja meg.
DateTime Timestamp		A vélemény felvételének ideje
String review_category		A termék kategóriája, ez felgyorsítja a keresést nagy adatbázis esetén a partíciós rendszer miatt
String review_name		A véleményt hagyó személy neve, több szerepet kapna felhasználó menedzsmenttel
Double review_rating		A vélemény pontszáma
String review_text		A vélemény szövegezése(kevesebb, mint 500 karakter)

Log Table			Minden termékre irányuló GET request után az adott felhasználó cselekedete rögzítése kerül,
				hogy le lehessen ellenőrizni, hogy megtekintette-e a legutóbbi véleményeket.
				Az adat a POST vélemény rögzítő request után törlődik, így nem telik meg az adatbázis.

String PartitionKey		Termék azonosító
String RowKey			Kapcsolat egyedi azonosítója
DateTime Timestamp		A log felvételének ideje.

Mi kellene még ahhoz, hogy kész legyen az éles környezetben való használatra?

-Nagy volumenű tesztek és esetleges problémák fixálása.
-A programkódban lévő érzékeny adatok titkosítása.
-A Log táblából a régebbi adatok törlése, így ha csak GET request történik akkor se marad ott egy bizonyos időnél tovább az adat.
-Https használata, hogy az adatátvitel titkosított legyen.
-User-management, így megoldható lenne, hogy egy felhasználó tudja módosítani a véleményét, valamint termékenként csak 1-et hagyjon.
