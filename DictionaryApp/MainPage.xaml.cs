using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace DictionaryApp
{
    public partial class MainPage : ContentPage
    {
        private List <string> dictionaries;

        public MainPage()
        {
            InitializeComponent();

            dictionaries = new List<string>();
        }
        private async void CreateDictionary_Clicked(object sender, EventArgs e)
        {
            string dictionaryName = await DisplayPromptAsync("Создать словарь", "Введите название словаря");
            if (!string.IsNullOrWhiteSpace(dictionaryName))
            {
                dictionaries.Add(dictionaryName);
                SaveDictionariesToJson();
            }
        }

        private async void OpenDictionary_Clicked(object sender, EventArgs e)
        {
            LoadDictionariesFromJson();
            string selectedDictionary = await DisplayActionSheet("Открыть словарь", "Отмена", null, dictionaries.ToArray());
            if (!string.IsNullOrWhiteSpace(selectedDictionary))
            {
                var editPage = new EditDictionaryPage(selectedDictionary);
                editPage.DictionaryDeleted += EditPage_DictionaryDeleted;
                await Navigation.PushAsync(editPage);
            }
        }

        private void EditPage_DictionaryDeleted(string dictionaryName)
        {
            dictionaries.Remove(dictionaryName);
            SaveDictionariesToJson();
        }

        private void SaveDictionariesToJson()
        {
            string json = JsonConvert.SerializeObject(dictionaries);
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dictionaries.json");
            File.WriteAllText(filePath, json);
        }

        private void LoadDictionariesFromJson()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dictionaries.json");
            if (File.Exists(filePath))
            {

                string json = File.ReadAllText(filePath);
                dictionaries = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }
    }
}