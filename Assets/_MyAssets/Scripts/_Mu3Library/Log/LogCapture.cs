using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Mu3Library.Log {
    public class LogCapture : MonoBehaviour {
        [SerializeField] private LogMessage logObj;
        [SerializeField] private Transform logParent;
        private List<LogMessage> logs = new List<LogMessage>();

        [SerializeField] private TextMeshProUGUI logCountText;
        private Dictionary<LogType, int> logCount = new Dictionary<LogType, int>();



        private void OnEnable() {
            Application.logMessageReceived += LogMessageReceived;
        }

        private void OnDisable() {
            Application.logMessageReceived -= LogMessageReceived;
        }

        #region Utility
        public void ClearLog() {
            for(int i = 0; i < logs.Count; i++) {
                UnityObjectPoolManager.Instance.AddObject(logs[i]);
            }
            logs.Clear();

            logCountText.text = "";
            logCount.Clear();
        }
        #endregion

        #region Action
        private void LogMessageReceived(string condition, string stackTrace, LogType type) {
            LogMessage lm = UnityObjectPoolManager.Instance.GetObject<LogMessage>();
            if(lm == null) {
                lm = Instantiate(logObj);
            }
            lm.transform.SetParent(logParent);
            lm.transform.localScale = Vector3.one;
            lm.gameObject.SetActive(true);

            lm.SetLog(type, $"{condition}\nStack: {stackTrace}");

            logs.Add(lm);

            AddLogCount(type);
        }
        #endregion

        StringBuilder logCountBuilder = new StringBuilder();
        private void AddLogCount(LogType type) {
            int count = -1;
            if(logCount.TryGetValue(type, out count)) {
                logCount[type] = count + 1;
            }
            else {
                logCount.Add(type, 1);
            }

            logCountBuilder.Clear();
            int keyCount = logCount.Keys.Count;
            int index = 0;
            foreach(LogType t in logCount.Keys) {
                logCountBuilder.Append($"{t}: {logCount[t]}");

                index++;
                if(index < keyCount) {
                    logCountBuilder.Append(", ");
                }
            }

            logCountText.text = logCountBuilder.ToString();
        }
    }
}