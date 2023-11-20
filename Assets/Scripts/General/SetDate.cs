using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


namespace Bercetech.Games.Fleepas
{
    public class SetDate : MonoBehaviour
    {
        
        private string _dateFormat = FleepasFormats.DateFormat;
        [SerializeField]
        private int _yearsToAdd = 1;
        [SerializeField]
        private int _daysToAdd = -1;
        private DateTime _selectedDate;
        public DateTime SelectedDate => _selectedDate;
        void Start()
        {
            _selectedDate = DateTime.Today
                .AddYears(_yearsToAdd)
                .AddDays(_daysToAdd);
            GetComponent<TextMeshProUGUI>().text = _selectedDate.ToString(_dateFormat);
        }

    }
}