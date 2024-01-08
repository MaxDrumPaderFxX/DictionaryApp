using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;

namespace DictionaryApp
{
    public partial class EditDictionaryPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<WordEntry> words;
        private WordEntry selectedWord;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WordEntry> Words
        {
            get { return words; }
            set
            {
                words = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Words)));
            }
        }

        public WordEntry SelectedWord
        {
            get { return selectedWord; }
            set
            {
                selectedWord = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedWord)));
            }
        }

        public delegate void DictionaryDeletedEventHandler(string dictionaryName);
        public event DictionaryDeletedEventHandler DictionaryDeleted;

        public EditDictionaryPage(string dictionaryName)
        {
            InitializeComponent();
            Title = dictionaryName;
            LoadDictionaryFromJson();
            BindingContext = this;
        }

        private async void AddWord_Clicked(object sender, EventArgs e)
        {
            string word = await DisplayPromptAsync("Добавить слово", "Введите слово");
            if (!string.IsNullOrWhiteSpace(word))
            {
                string translation = await DisplayPromptAsync("Добавить слово", "Введите перевод");
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    var newWord = new WordEntry { Key = word, Value = translation, AddedDate = DateTime.Now };
                    Words.Add(newWord);
                    SaveDictionaryToJson();
                }
            }
        }

        private async void EditWord_Clicked(object sender, EventArgs e)
        {
            if (SelectedWord != null)
            {
                string word = await DisplayPromptAsync("Редактировать слово", "Изменить слово", initialValue: SelectedWord.Key);
                string translation = await DisplayPromptAsync("Редактировать слово", "Изменить перевод", initialValue: SelectedWord.Value);

                if (!string.IsNullOrWhiteSpace(translation) && !SelectedWord.Value.Equals(translation))
                {
                    SelectedWord.Value = translation;
                    Words[Words.IndexOf(SelectedWord)] = SelectedWord;
                    SaveDictionaryToJson();
                }
                else if (!string.IsNullOrWhiteSpace(word) && !SelectedWord.Key.Equals(word))
                {
                    if (!Words.Any(w => w.Key.Equals(word)))
                    {
                        Words.Remove(SelectedWord);
                        var updatedWord = new WordEntry { Key = word, Value = translation, AddedDate = DateTime.Now };
                        Words.Add(updatedWord);
                        SelectedWord = updatedWord;
                        SaveDictionaryToJson();
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Слово с таким ключом уже есть в словаре", "ОК");
                    }
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Вы должны выбрать слово для редактирования.", "ОК");
            }
        }

        private void DeleteWord_Clicked(object sender, EventArgs e)
        {
            if (SelectedWord != null)
            {
                Words.Remove(SelectedWord);
                SelectedWord = null;
                SaveDictionaryToJson();
            }
            else
            {
                DisplayAlert("Ошибка", "Вы должны выбрать слово для удаления.", "ОК");
            }
        }

        private async void DeleteDictionary_clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Удаление словаря", $"Вы действительно хотите удалить словарь '{Title}'?", "Удалить", "Отмена");

            if (confirm)
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{Title}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    DictionaryDeleted?.Invoke(Title);
                    await DisplayAlert("Словарь удален", "Словарь был успешно удален.", "ОК");

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Файл словаря не найден.", "ОК");
                }
            }
        }

        private void SaveDictionaryToJson()
        {
            var dictionary = Words.ToDictionary(
                w => w.Key,
                w => new { Translation = w.Value, AddedDate = w.AddedDate.ToString("o") }
            );
            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{Title}.json");
            File.WriteAllText(filePath, json);
        }

        public class JsonWordEntry
        {
            public string Translation { get; set; }
            public string AddedDate { get; set; }
        }

        private void LoadDictionaryFromJson()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{Title}.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, JsonWordEntry>>(json);
                Words = new ObservableCollection<WordEntry>(dictionary.Select(kv => new WordEntry { Key = kv.Key, Value = kv.Value.Translation, AddedDate = DateTime.Parse(kv.Value.AddedDate, null, System.Globalization.DateTimeStyles.RoundtripKind) }));
            }
            else
            {
                Words = new ObservableCollection<WordEntry>();
            }
        }

        private void SortAlphabetically_Clicked(object sender, EventArgs e)
        {
            Words = new ObservableCollection<WordEntry>(Words.OrderBy(w => w.Key));
        }

        private void SortByDate_Clicked(object sender, EventArgs e)
        {
            Words = new ObservableCollection<WordEntry>(Words.OrderBy(w => w.AddedDate));
        }
    }
}