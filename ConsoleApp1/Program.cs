using Microsoft.Data.SqlClient;

//основной текст программы
string? word;
do
{
    Console.WriteLine("Enter the word to translate or 0 to exit");
    word = Console.ReadLine();
    if (word == "0" ) break;

    using (DBReader dBReader = new DBReader())
    {
        //ищется слово в базе данных
        if (!dBReader.Read(word))
        {
            //если слово не находится, то предлагается вывести LIKE %word% слова по запросу пользователя
            Console.WriteLine("Would you find similar words? (1 - yes; anykey - no) ");
            if (Console.ReadLine() == "1")
            {
                dBReader.ReadLike(word);
            }
        }
    }
} while (true);


public class DBReader : IDisposable
{

    private string _connectionPath = "Server=localhost;Database=Dictionary;Trusted_Connection=True;Encrypt=False;";
    private SqlConnection _connection;

    public DBReader()
    {
        //связь с БД
        _connection = new SqlConnection(_connectionPath);
        _connection.Open();
    }

    public bool Read (string ? word, string language = "RUS")
    {
        SqlCommand command = new SqlCommand($"SELECT Translation FROM Translations  JOIN Words ON Words.ID = Translations.WordID  JOIN Languages ON Languages.Id = Translations.LanguageID   WHERE Word = '{word}' AND LanguageName = '{language}'", _connection);
        SqlDataReader reader = command.ExecuteReader();
        Console.Clear();
        
        //если запрос на чтение пуст (ничего не найдено), то сообщаем пользователю и выходим из метода
        if (!reader.HasRows)
        {
            Console.WriteLine($"'{word}' not found in the dictionary :(");
            reader.Close();
            return false;
        }

        //если запрос показа значения, то выводим их на экран
        Console.Write(word + " - ");
        while (reader.Read())
        {
            string translation = reader.GetString(0);
            Console.Write($"{translation}; ");
        }
        Console.WriteLine();
        reader.Close();
        return true;
    }

    //метод ReadLike делает запрос на основании подобности %LIKE% из базы данных
    public void ReadLike(string? word, string language = "RUS")
    {
        SqlCommand command = new SqlCommand($"SELECT Word, Translation FROM Translations  JOIN Words ON Words.ID = Translations.WordID  JOIN Languages ON Languages.Id = Translations.LanguageID   WHERE Word LIKE '{word}%' AND LanguageName = '{language}'", _connection);
        SqlDataReader reader = command.ExecuteReader();
        Console.Clear();
        if (!reader.HasRows)
        {
            Console.WriteLine($"'{word}-LIKE' not found in the dictionary :(");
            reader.Close();
            return;
        }
        Console.Write(word + " - ");
        while (reader.Read())
        {
            word = reader.GetString(0);
            string translation = reader.GetString(1);
            Console.WriteLine($"{word} - {translation};");
        }
        reader.Close();
    }

    public void Dispose()
    {
        _connection.Close();
    }
}


