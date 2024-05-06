Projekt ma na celu wygenerowanie 2d procedurlanego lochu, który składa się z wczęsniej przygotowanych prefabów pomieszczeń oraz korytarzy łączcych te pomieszczenia. Projekt został wykonany w języku C# w środowisku 
unity. Wykorzystuję on takie algorytmy jak Delaunay triangulation, znajdowanie minimum spanning tree oraz A* search algorithm. Koncept projektu został zaczerpnięty z pierwszej części tego filmiku: 
https://www.youtube.com/watch?v=rBY2Dzej03A&t=257s&ab_channel=Vazgriz. W dalszej części postaram się w skrócię opisać koljene kroki przedstawione na filmiku w mojej implementacji tego algorytmu.

Pierwszym krokim jest umieszczenie pomieszczeń w jakichś losowych punktach na powierzchni o wcześniej ustalonych rozmiarach. Wielkość początkowej powierzhni w dalszym etapie decyduje między innymi o tym jak długi i kręty 
będzie pojedynczy korytarz. Jako, że moim celem jest osiągnięcie względnie krótkich i jak najbardziej krętych korytarzy to ustawiam rozmiar początkowej przestrzeni na względnie małą (na tyle że większość pokoi będzie na siebie nachodzić)

[zdiecie nakladającyhc się pomieszczeń i wynikające z tego korytarze oraz oddalone od siebie pokoje i dluygie korytarze]

Następnym krokiem jest rozsunięcie pomieszczeń od siebie. Dzieje się to poprzez porównnywanie dwóch kolejnych pomieszczeń. Jeśli na siebie nachodzą to wybierana jest jeda długość decydująca o odleglości o jaką zostanie 
przesunięty pokój. Sprawdzam odległość miedzy środkami pokojów oraz dłogóść i szerokość każdego z nich. Nawiększa wartość zostaje wybrana jako wielkość przesunięcia. Następnie pokój jest przesówany losowo w jedną z czterach 
stron o tą właśnie wartość. Jeżeli wcześniej wspomniana startowa powierzchnia na której rozrzucone zostaną pomieszczenia będzię duża, największą wartością zawsze będzie odległość miedzy pokojami i to właśnie ona spowoduję 
ze korytarzę będą długie i nudne 

[zdiecie rozsunietych ]

Teraz można sporządzić Delaunay Triangulation (https://en.wikipedia.org/wiki/Delaunay_triangulation)
Dzięki temu algorytmowi znajdziemy optymalne połączenia pokoi miedzy sobą.

Zbiorem punktów z którego zostanie sporządzony graf jest zbiór w współrzędnych dla każdego pokoju które określają w którym miejscu znajdują się drzwi a co za tym idzie to do tego miejsca ma prowadzic 
kotytarz. Współrzędne te są obliczane dzięki loklanym współrzędnym drzwi dla kazdego pokoju oraz jego współrzednych środka w ogólnej przestrzeni.

[screen door location, screen pokoju z kreskami pokazującymi gdzie jest door location]


Celem algorytmu jest stworzenie z listy punktów tak zwanej triangulacji czyli zbioru trójkóatów, które cechują się tym, że żaden wierzchołek nie leży wewnątrz okręgu opisanego w dowolnym trójkącie w zbiorze.
Pierwszym krokiem jest stworzenie pierwszego tak zwanego super trójkąta, który jest na tyle duży aby wszystkie punkty znajdowały się w jego wnętrzu. W tym celu sprawdzam największę odległości w pionie i poziomie miedzy 
punktami i za pomocą wielokrtoności tych odległości wyznaczam położenie punktów tego trójkąta. Następnie iteruję po wszystkich punktach dodając je to triangulacje czyli tworząc z nich trójkąty sprawdzając czy 
spełniają wcześniej wymieniony warunek. Po wykananiu algorytmu otrzymuje zbiór krawędzi należacych do trójkątów, którę bardzo często się powtarzają dla tego usuwam duplikaty. Jeżeli w zbiorzę znajdują się dwie krawędzie 
miedzy tymi samymi punktami (tzn z punktu A do B oraz z punktu B do A) to równierz kasuje jedną z tych krawędzi.

[zdiecie triangulacji]



Następnym krokiem będzie znalezienie gównej drogi w lochu. Ma być to optymalna trasa prowadząca przez wszystkie pomieszczenia. Pomoże w tym tak zwane MST (Minimum spanning tree). Jest to minimlane podrzewo które cechuję 
się tym że zawiera wszytkie punkty w grafie połączone krawędziami o możliwie minimalnych wagach w taki sposób a by nie tworzyć cykli. W moim przypadku graf nie jest ważony więc podczas wybierania odpowiednich krawędzi 
zawsze wybierana jest pierwsza możliwa. Sposób znajdowania MST polega na utworzeniu listy odwiedzonych punktów gdzie na początku znajduję się tylko jeden wybrany losowo punkt. Następnie tak długo jak lista wierzchołków 
odwiedzonych nie zawiera wszystkich wierzchołków wyszukuje krawędzi która spelnia warunek którym jest łączenie punktu odwiedzonego z punktem jeszcze nie odwiedzonym. Gdy taka krawędź zostanie znaleziona zostaje ona dodana do MST a 
nowy wierzchołek zostaje dodany do listy odwiedzoncych. 


[zdiecie mst]

Takie drzewo tworzy loch z niewymagającymi korytarzami dla tego następnym krokiem jest dodanie pewnych niewykorzystanych jeszcze krawędzi co spowoduje utworzenie cykli w grafie a co za tym idzie loch stanie się bardziej 
kręty. Iteruję po wszystkich krawędziach spoza mst i z pewną szansą dodaje je do mst (szansę tą można dostosować z poziomu inspektora w unity. Zazwyczaj wykorzystuje ok. 30%) 

[zdiecie nowego mst]
 

Teraz nic juz nie stoi na przeszkodzie aby wytyczyć odpowiednie scieżki między pokojami. Wykorzystany do tego został A* search algorithm. (https://en.wikipedia.org/wiki/A*_search_algorithm)
Do tego celu stworzyłem odpowiednią siatkę pozawalającą na szukanie scieżki. Jeden 
kwadrat przedstawiał jeden kawałek korytarza który później reprezentowany będzie jako jakiś prefab. Żeby ta technika działała trzeba dbać o to aby każdy prefab pokoju równierz był odpowiedniej wielkości i wpasowywał się w 
siatkę.

[tutaj screen siatki z pokojami ]




