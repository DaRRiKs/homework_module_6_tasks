/* Курс: Шаблоны проектирования приложений

Тема: Модуль 06 Паттерны поведения. Стратегия. Наблюдатель

Задача:
Необходимо реализовать приложение на C#, которое моделирует поведение разных типов оплаты (например, банковская карта, PayPal, криптовалюта и т.д.) с использованием порождающего паттерна Стратегия. 
Цель — предоставить гибкий способ замены алгоритмов оплаты без изменения кода клиентских классов.
Описание:
1.	Контекст: Контекст будет представлять собой класс, который выполняет процесс оплаты, используя различные стратегии.
2.	Стратегии: Каждая стратегия будет представлять собой отдельную реализацию метода оплаты, например, оплата банковской картой, через PayPal или криптовалютой.
3.	Клиент: Клиентский код должен иметь возможность переключать стратегии оплаты по необходимости.
Шаги выполнения:
1.	Создайте интерфейс IPaymentStrategy, который будет определять метод Pay
2.	Создайте несколько классов-стратегий, которые реализуют интерфейс IPaymentStrategy, например, стратегии для оплаты банковской картой, через PayPal и с помощью криптовалюты.
3.	Создайте класс контекста PaymentContext, который будет работать с разными стратегиями оплаты
4.	Напишите клиентский код, который будет задавать различные стратегии оплаты и выполнять их

Требования к выполнению:
•	Реализуйте минимум три различных стратегии оплаты.
•	Реализуйте возможность выбора стратегии оплаты в зависимости от входных данных пользователя.
•	При выполнении программы должна быть возможность легко переключаться между разными способами оплаты без изменения структуры программы. */
/* using System;
using System.Globalization;

public interface IPaymentStrategy
{
    string Method { get; }
    bool Pay(decimal amount, out string message);
}

public class CardPaymentStrategy : IPaymentStrategy
{
    public string Method => "Card";
    private readonly string _cardNumber;
    private readonly string _holder;
    private readonly string _cvv;

    public CardPaymentStrategy(string cardNumber, string holder, string cvv)
    {
        _cardNumber = (cardNumber ?? "").Replace(" ", "");
        _holder = holder ?? "";
        _cvv = cvv ?? "";
    }

    public bool Pay(decimal amount, out string message)
    {
        if (_cardNumber.Length < 12 || _cardNumber.Length > 19)
        {
            message = "Ошибка: номер карты некорректный.";
            return false;
        }
        if (_cvv.Length < 3 || _cvv.Length > 4)
        {
            message = "Ошибка: CVV некорректный.";
            return false;
        }
        if (amount <= 0)
        {
            message = "Сумма должна быть > 0.";
            return false;
        }

        decimal fee = Math.Round(amount * 0.02m, 2);
        decimal total = amount + fee;

        message = $"[CARD] Держатель: {_holder}. Сумма: {amount} + комиссия 2% = {total}. Оплата проведена успешно.";
        return true;
    }
}

public class PayPalPaymentStrategy : IPaymentStrategy
{
    public string Method => "PayPal";
    private readonly string _email;

    public PayPalPaymentStrategy(string email)
    {
        _email = email ?? "";
    }

    public bool Pay(decimal amount, out string message)
    {
        if (string.IsNullOrWhiteSpace(_email) || !_email.Contains("@"))
        {
            message = "Ошибка: некорректный e-mail PayPal.";
            return false;
        }
        if (amount <= 0)
        {
            message = "Сумма должна быть > 0.";
            return false;
        }

        decimal fee = Math.Round(amount * 0.035m + 0.30m, 2);
        decimal total = amount + fee;

        message = $"[PayPal] Аккаунт: {_email}. Сумма: {amount} + комиссия 3.5%+0.30 = {total}. Оплата проведена.";
        return true;
    }
}

public class CryptoPaymentStrategy : IPaymentStrategy
{
    public string Method => "Crypto";
    private readonly string _network;
    private readonly string _wallet;

    public CryptoPaymentStrategy(string network, string wallet)
    {
        _network = (network ?? "").ToUpperInvariant();
        _wallet = wallet ?? "";
    }

    public bool Pay(decimal amount, out string message)
    {
        if (amount <= 0)
        {
            message = "Сумма должна быть > 0.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(_wallet))
        {
            message = "Ошибка: не указан адрес кошелька.";
            return false;
        }

        decimal networkFee = _network switch
        {
            "BTC" => 3.00m,
            "ETH" => 2.00m,
            "USDT" => 1.00m,
            _ => 1.50m
        };

        decimal total = amount + networkFee;

        message = $"[CRYPTO] Network={_network}, Wallet={Mask(_wallet)}. Сумма: {amount} + сеть {networkFee} = {total}. Транзакция отправлена.";
        return true;
    }

    private static string Mask(string w)
    {
        if (w.Length <= 6) return w;
        return w.Substring(0, 4) + "..." + w.Substring(w.Length - 2);
    }
}

public class PaymentContext
{
    private IPaymentStrategy _strategy;

    public void SetStrategy(IPaymentStrategy strategy)
    {
        _strategy = strategy;
    }

    public bool Process(decimal amount, out string message)
    {
        if (_strategy == null)
        {
            message = "Стратегия не выбрана.";
            return false;
        }
        return _strategy.Pay(amount, out message);
    }
}

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var context = new PaymentContext();

        Console.WriteLine("Платёжная система (Strategy)");
        Console.WriteLine("Доступные способы: 1) card  2) paypal  3) crypto");
        Console.Write("Введите способ оплаты: ");
        string method = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

        Console.Write("Введите сумму (пример 199.99): ");
        string raw = (Console.ReadLine() ?? "").Trim().Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            Console.WriteLine("Некорректная сумма.");
            return;
        }

        switch (method)
        {
            case "1":
            case "card":
                Console.Write("Номер карты: ");
                var card = Console.ReadLine();
                Console.Write("Держатель (имя на карте): ");
                var holder = Console.ReadLine();
                Console.Write("CVV: ");
                var cvv = Console.ReadLine();
                context.SetStrategy(new CardPaymentStrategy(card!, holder!, cvv!));
                break;

            case "2":
            case "paypal":
                Console.Write("E-mail PayPal: ");
                var email = Console.ReadLine();
                context.SetStrategy(new PayPalPaymentStrategy(email!));
                break;

            case "3":
            case "crypto":
                Console.Write("Сеть (BTC/ETH/USDT/...): ");
                var net = Console.ReadLine();
                Console.Write("Адрес кошелька: ");
                var wallet = Console.ReadLine();
                context.SetStrategy(new CryptoPaymentStrategy(net!, wallet!));
                break;

            default:
                Console.WriteLine("Неизвестный способ оплаты.");
                return;
        }

        if (context.Process(amount, out string msg))
            Console.WriteLine("✔ Успех: " + msg);
        else
            Console.WriteLine("✖ Ошибка: " + msg);

        Console.WriteLine("\n— Переключаемся на PayPal без изменения остального кода —");
        context.SetStrategy(new PayPalPaymentStrategy("student@example.com"));
        context.Process(50m, out string msg2);
        Console.WriteLine(msg2);
    }
} */

/* Задача:
Необходимо реализовать приложение на C#, которое демонстрирует работу паттерна Наблюдатель (Observer). Приложение будет моделировать ситуацию, когда разные подписчики получают уведомления от одного субъекта, например, обновления курсов валют или изменение цен на акции.
Описание:
1.	Субъект: Субъект будет представлять объект, за состоянием которого наблюдают другие объекты. При изменении состояния субъекта, он уведомляет всех своих подписчиков.
2.	Наблюдатели: Наблюдатели — это объекты, которые подписаны на изменения состояния субъекта. Каждый наблюдатель реагирует на изменения по-своему.
3.	Клиент: Клиентский код должен позволять добавлять и удалять наблюдателей, а также обновлять их при изменении состояния субъекта.
Шаги выполнения:
1.	Создайте интерфейс IObserver, который будет представлять наблюдателя
2.	Создайте интерфейс ISubject, который будет представлять субъекта
3.	Реализуйте класс субъекта CurrencyExchange, который хранит информацию о курсах валют и уведомляет наблюдателей об изменении курсов
4.	Создайте несколько классов-наблюдателей, которые реализуют интерфейс IObserver. Каждый наблюдатель будет получать обновления и реагировать по-своему
5.	Напишите клиентский код, который будет взаимодействовать с субъектом и наблюдателями


Требования к выполнению:
•	Реализуйте минимум три разных наблюдателя, каждый из которых по-своему обрабатывает обновления.
•	Обеспечьте возможность добавления и удаления наблюдателей.
•	Реализуйте уведомление наблюдателей при изменении состояния субъекта. */

