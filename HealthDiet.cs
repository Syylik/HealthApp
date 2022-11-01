namespace App.Game
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    [System.Serializable]
    public class Dish
    {
        public Sprite dishSprite;   //Фото блюда
        public string name;
        public int calorie;
        public Weekday whichDayOfWeek; //день недели, когда нужно есть это блюдо
        public Dish() { }
        public Dish(string dishName, int dishCalorie, Sprite dishImage, Weekday dayOfWeek)
        {
            name = dishName;
            calorie = dishCalorie;
            dishSprite = dishImage;
            whichDayOfWeek = dayOfWeek;
        }
    }
    public enum Weekday
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 0
    }
    public class HealthDiet : MonoBehaviour
    {
        private Weekday _todayDayOfWeek; //сегоднешний день недели
        [SerializeField] private Dish[] _allDishes;  //все блюда

        [Header("UI")]
        [SerializeField] private TMP_Text _dishName;
        [SerializeField] private TMP_Text _dishCalorie; 
        [SerializeField] private Image _dishImage;
        private void Start()
        {
            var date = new DateTime();
            date = DateTime.Today;      
            _todayDayOfWeek = (Weekday)(int)date.DayOfWeek;   //получаем день недели 
            SetDishUI(TryGetDishForDay());
        }
        private Dish TryGetDishForDay() //Возвращает еду, для дня недели
        {
            if(_allDishes.Length > 0)
            {
                List<Dish> dishiesForDay = new List<Dish>();
                foreach(var dish in _allDishes)
                {
                    if(dish.whichDayOfWeek == _todayDayOfWeek)
                        dishiesForDay.Add(dish);
                }
                if(dishiesForDay.Count > 0)
                {
                    var randDish = new Dish();
                    randDish = dishiesForDay[UnityEngine.Random.Range(0, dishiesForDay.Count)];
                    return randDish;
                }
                else return new Dish();
            }
            else return new Dish();
        }
        private void SetDishUI(Dish dish)
        {
            _dishName.text = dish.name;
            _dishCalorie.text = $"{dish.calorie} calorie";
            _dishImage.sprite = dish.dishSprite;   
        }
    }
}