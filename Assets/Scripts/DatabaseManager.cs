// using System;
// using Random=UnityEngine.Random;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase.Database;
// using Firebase.Auth;
// using Google.MiniJSON;
// using TMPro;
// using UnityEditor.Build.Content;
// using System.Net.Mail;
// using System.Text.RegularExpressions;
// using Firebase.Extensions;
// using Unity.VisualScripting;
// using UnityEngine.Rendering;
//
// public class DatabaseManager : MonoBehaviour
// {
//     private DatabaseReference reference;
//     private Firebase.Auth.FirebaseAuth auth;
//
//     void Start()
//     {
//         reference = FirebaseDatabase.DefaultInstance.RootReference;
//         auth = FirebaseAuth.DefaultInstance;
//         
//         reference.Child("players").ValueChanged += HandlePlayerValueChanged;
//     }
//
//     void HandlePlayerValueChanged(object send, ValueChangedEventArgs
//         args)
//     {
//         if (args.DatabaseError != null)
//         {
//             Debug.LogError(args.DatabaseError.Message);
//             return;
//         }
//
//         numberOfPlayers = (int)args.Snapshot.ChildrenCount;
//         playerCountText.text = numberOfPlayers.ToString();
//     }
//
//     public void SignUp()  
//     {
//         Debug.Log("Submit Values (Signup)");
//         if (!string.IsNullOrWhiteSpace(usernameText.text) && !string.IsNullOrWhiteSpace(emailText.text) && !string.IsNullOrWhiteSpace(passwordText.text))
//         {
//             if (isValidEmail(emailText.text))
//             {
//                 if (ValidatePassword(passwordText.text))
//                 {
//                     auth.CreateUserWithEmailAndPasswordAsync(emailText.text, passwordText.text).ContinueWith(task => {
//                     if (task.IsCanceled) {
//                         Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
//                         return;
//                     }
//                     if (task.IsFaulted) {
//                         validationText.text = "Weak Password (Include letters and numbers)";
//                         Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
//                         return;
//                     }
//
//                     // Firebase user has been created.
//                     Firebase.Auth.AuthResult result = task.Result;
//                     loggedIn = true;
//                     Debug.LogFormat("auth user created successfully: {0} ({1})",
//                         result.User.DisplayName, result.User.UserId);
//                     });
//                     if (loggedIn == true)
//                     {
//                         signUpScreen.SetActive(false);
//                         homeScreen.SetActive(true);
//
//                         playerScore = Random.Range(1, 10000);
//
//                         WriteNewPlayer(
//                             usernameText.text,
//                             emailText.text,
//                             passwordText.text,
//                             "Swordsman",
//                             "2012-04-23T18:25:43.511Z",
//                             "2024-06-23T21:24:13.511Z",
//                             playerScore,
//                             0,
//                             764,
//                             new string[] { "Gnash", "Teleport", "Sand Slash" },
//                             new string[] { "Sword", "Book", "Shield", "Iron Helmet", "Chainmail Chestplate" },
//                             true,
//                             1,
//                             25);
//                         DisplayData();
//                         usernameText.text = "";
//                         emailText.text = "";
//                         passwordText.text = "";
//                     }
//                 }
//                 else
//                 {
//                     validationText.text = "Weak Password (Requires upper case letters, lower case letters, numbers)";
//                 }
//             }
//             else
//             {
//                 validationText.text = "Invalid Email";
//             }
//         }
//         else
//         {
//             validationText.text = "Please fill all fields";
//         }
//     }
//
//     public void Login()
//     {
//         Debug.Log("Submit Values (Login)");
//         auth.SignInWithEmailAndPasswordAsync(loginEmailText.text, loginPasswordText.text).ContinueWith(task => {
//             if (task.IsCanceled) {
//                 Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
//                 return;
//             }
//             if (task.IsFaulted)
//             {
//                 loginValidationText.text = "Wrong Username or Password";
//                 Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
//                 return;
//             }
//             loggedIn = true;
//             Firebase.Auth.AuthResult result = task.Result;
//             Debug.LogFormat("User signed in successfully: {0} ({1})",
//                 result.User.DisplayName, result.User.UserId);
//         });
//         if (loggedIn == true)
//         {
//             loginScreen.SetActive(false);
//             homeScreen.SetActive(true);
//             emailText.text = "";
//             passwordText.text = "";
//         }
//     }
//
//     public void Logout()
//     {
//         Debug.Log("Log Out!");
//         loggedIn = false;   
//         auth.SignOut();
//     }
//     
//     private bool isValidEmail(string email) {
//         try {
//             var addr = new MailAddress(email);
//             return true;
//         }
//         catch {
//             return false;
//         }
//     }
//
//     private bool ValidatePassword(string password)
//     {
//         // Set the minimum password length
//         int minLength = 8;
//
//         // Check the password length
//         if (password.Length < minLength)
//             return false;
//
//         // Check if the password contains at least one uppercase letter
//         if (!Regex.IsMatch(password, "[A-Z]"))
//             return false;
//
//         // Check if the password contains at least one lowercase letter
//         if (!Regex.IsMatch(password, "[a-z]"))
//             return false;
//
//         // Check if the password contains at least one digit
//         if (!Regex.IsMatch(password, "[0-9]"))
//             return false;
//         
//         return true;
//     }
//
//     private void WriteNewPlayer(string name, string email, string password, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int rocksShot, string[] achievements, int level)
//     {
//         PlayerData player = new PlayerData(name, email, password, creationDate, lastLoginDate, highScore, gamesPlayed, rocksShot, achievements, level);
//
//         var playerReference = reference.Child("players").Push().Key;
//         playerID = playerReference.ToString();
//         string json = JsonUtility.ToJson(player);
//         reference.Child("players").Child(playerReference).SetRawJsonValueAsync(json);
//         reference.Child("leaderboard").Child(usernameText.text).SetRawJsonValueAsync(playerScore.ToString());
//     }
//
//     public void WriteNewScore(int score)
//     {
//         DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
//         dbReference.Child("players/" + playerID + "/score").SetValueAsync(score);
//         DatabaseReference reference =
//             FirebaseDatabase.DefaultInstance.GetReference("players/");
//         Dictionary<string, object> childUpdates = new Dictionary<string, object>();
//         childUpdates[playerID + "/score"] = score;
//         reference.UpdateChildrenAsync(childUpdates);
//     }
// }
