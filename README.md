# StochasticApi

Описание решения. 
Для работы с таким количеством событий одной только памяти нам не хватит.
Поэтому Api в конструкторе принимает интерфейс IDataProvider, который обеспечивает доступ к массиву данных, где он ни был - в памяти, на диске или в базе.
Я реализовал два DataProvider'a:
- ArrayDataProvider служит для хранения данных в памяти в массиве. Он прекрасно подходит для отладки, но, к сожалению, сильно ограничен по размерам - на количестве элементов больше 10^6 можно получить OutOfMemory. 
- MmfDataProvider служит для хранения данных на диске. При этому мы будем читать из него только порции данных, все данные из него нам не нужны. Идеально подходит для работы с большим количеством данных. О скорости работы с ним я расскажу чуть позже.

По условию задачи необходимо реализовать api для получения всех евентов попавших в промежуток между начальным и конечным тиками. Таким образом интерфейс для получения евентов будет выглядеть следующим образом: 
    public interface IEventApi
    {
        IEnumerable<PayloadEvent> GetEvents(long startTick, long stopTick);
    }
 
Я его реализовал, но, к сожалению, в задаче построения графика он нам слабо поможет. Создание всех евентов, попавших в указанный промежуток неоптимально не по памяти, не по времени. 
Поэтому оставляем эту реализацию и переходим к следующей версии:
	public interface IDensityApi:IDisposable
    {
         List<DensityInfo> GetDensityInfo(long start, long stop, long groupInterval);
    }
	
При работе с этим апи мы не будем держать в памяти наши евенты(точнее будем, но не дольше, чем для того, чтобы считать из них информацию о тиках). 
Т.к. все множество наших евентов упорядочено, то можно приминить бинарный поиск для поиска ближайщим к запрашиваемым границам евенты. 
Дальше мы не будем их читать, нам нужно только количество. groupInterval нужен для отображения столбчатой диаграммы. Если описать алгоритм в двух словах, то мы разбиваем весь промежуток на N равных промежутков и с помощью бинарного поиска находим индексы евентов ближайщих к границам, далее считаем количество евентов между индексами и получаем значение количества евентов в промежутке. 

Судя по картинке один с максимальным масштабом мы хотим отображать не просто плотность на промежутке, а еще с корректным отображением начала и конца. Т.е. если у нас есть промежутов 10 секунд, и я хочу сгруппировать евенты по 1 сек. То судя по картинке я не должен увидеть столбчатую диаграмму с началом в каждой секунде. Я должен увидеть столбы начало которых будет совпадать с временем первого евента. Т.е. если у меня есть 4 евента в временем 1.3с, 1.4с, 1.5с, 1.6. я не должен видеть столбец с 1 по 2 секунды высотой 4, я должен увидеть столбец высотой 4 с началом в 1.3 и с концом в 1.6. 
Поэтому DensityInfo содержит информацию о том, где начинаются его евенты и где заканчиваются

Отрисовка графика. 
Для графика я использую две картинки - сам график и таймлайн. Для первого нам не важен размер контрола, в котором мы его отрисовываем. Мы рисуем картинку с заданым расширением(маленькое даст "столбцы", а большое даст эффекты при сжимании). А дальше отдаем это под ответственность WPF. Да, как я сказал возможны маленькие дефекты, но они не сильно заметны. Зато мы избегаем большого количиства запросов на апи при изменении размера контрола. 
Второй таймлайн - там рисуется текст с временными отметками. Там такая логика уже не прокатит. Сжатый текст выглядит ужасно. Поэтому мы подписываемся на изменения размеров контрола и перерисовываем таймалайн каждый раз. 

Алгоритм работы с апи достаточно прост, каждый раз при скроле мыши или нажатии кнопок вправо, влево мы расчитываем границы видимой области в тиках. Далее идет запрос на апи для получения информации о плотностях на этом видимой области. 

Для работы и тестирования приложения я написал генераторы, которые могу геренить либо массив евентов в памяти, либо файл на диске содержайщий евенты. Поэтому при задании параметров будьте осторожны- возможны появление артефактов на диске(в папке с приложением)

Далее для покрытия тестами сделал несколько проектов с юнит тестами и один для тестов по производительности.
Тесты по производительности показывают, что есть возможность ускорить работу с апи для отрисовки плотностей. Но не уверен, что хватит на это времени

Основная идея рефакторинга, которая даст выигрыш по производительности - использовать данные загруженные из апи.
1. При уменьшении масштаба мы вычисляем видимые промежутки и каждый из них в параллельных потоках пытаемся пересчитать с новым groupInterval'ом
2. При движении вправо или влево мы также вычисляем видимые промежутки и запрашиваем только дельту для движения(причем зная индекс последнего/первого промежутка - мы можем оптимально задать границы для поиска)
3. При увеличении масштаба мы объединяем видимые промежутки и запрашиваем дельту слева и справа.
Я начал делать рефакторинг, но сходу сделать это не удалось, поэтому я вынес это на отдельную ветку. Она пока что не стабильна.
Результаты перф. тестов, которые побудили на рефакторинг:
Method	Mean	Error	StdDev	Median	Rank
GetDensityInfo	748.85 ms	23.924 ms	140.813 ms	746.45 ms	3
SplitDensityInfo	373.50 ms	3.965 ms	13.480 ms	370.63 ms	2
GetLeftAndRightInfo	67.74 ms	1.020 ms	6.893 ms	66.78 ms	1


Осознаю, что количества логирования и юнит тестов не достаточно, но списываю это на недостаток времени. 
