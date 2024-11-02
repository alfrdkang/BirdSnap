using System;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Google.MiniJSON;
using TMPro;
using UnityEditor.Build.Content;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Firebase.Extensions;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;
    private Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser user;

    private int numberOfPlayers;
    private bool loggedIn = false;
    
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject signupScreen;
    [SerializeField] private GameObject loginScreen;
    
    //Signup
    [SerializeField] private TMP_InputField signupUsernameInputField;
    [SerializeField] private TMP_InputField signupEmailInputField;
    [SerializeField] private TMP_InputField signupPasswordInputField;
    [SerializeField] private TMP_InputField signupConfirmPasswordInputField;
    [SerializeField] private TextMeshProUGUI signupValidationText;
    
    //Login
    [SerializeField] private TMP_InputField loginEmailInputField;
    [SerializeField] private TMP_InputField loginPasswordInputField;
    [SerializeField] private TextMeshProUGUI loginValidationText;


    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        
        reference.Child("players").ValueChanged += HandlePlayerValueChanged;
    }
    
    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
        if (auth.CurrentUser != user) {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn) {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    // Handle removing subscription and reference to the Auth instance.
    // Automatically called by a Monobehaviour after Destroy is called on it.
    void OnDestroy() {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void HandlePlayerValueChanged(object send, ValueChangedEventArgs
        args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        numberOfPlayers = (int)args.Snapshot.ChildrenCount;
        //playerCountText.text = numberOfPlayers.ToString();
    }

    public void SignUp()  
    {
        Debug.Log("Submit Values (Signup)");
        if (!string.IsNullOrWhiteSpace(signupUsernameInputField.text) && !string.IsNullOrWhiteSpace(signupEmailInputField.text) && !string.IsNullOrWhiteSpace(signupPasswordInputField.text) && !string.IsNullOrWhiteSpace(signupConfirmPasswordInputField.text))
        {
            if (isValidEmail(signupEmailInputField.text))
            {
                if (ValidatePassword(signupPasswordInputField.text))
                {
                    auth.CreateUserWithEmailAndPasswordAsync(signupEmailInputField.text, signupPasswordInputField.text).ContinueWith(task => {
                    if (task.IsCanceled) {
                        Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted) {
                        signupValidationText.text = "Weak Password (Include letters and numbers)";
                        Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    // Firebase user has been created.
                    Firebase.Auth.AuthResult result = task.Result;
                    loggedIn = true;
                    Debug.LogFormat("auth user created successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                    });
                    if (loggedIn)
                    {
                        signupScreen.SetActive(false);
                        homeScreen.SetActive(true);
                        WriteNewPlayer(
                            signupUsernameInputField.text,
                            signupEmailInputField.text,
                            signupPasswordInputField.text,
                            "",
                            "",
                            0,
                            0,
                            0,
                            new string[] { "New Snapper" },
                            1
                            );
                        signupUsernameInputField.text = "";
                        signupEmailInputField.text = "";
                        signupPasswordInputField.text = "";
                    }
                }
                else
                {
                    signupValidationText.text = "Weak Password (Requires upper case letters, lower case letters, numbers)";
                }
            }
            else
            {
                signupValidationText.text = "Invalid Email";
            }
        }
        else
        {
            signupValidationText.text = "Please fill all fields";
        }
    }

    public void Login()
    {
        Debug.Log("Submit Values (Login)");
        auth.SignInWithEmailAndPasswordAsync(loginEmailInputField.text, loginPasswordInputField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                loginValidationText.text = "Wrong Username or Password";
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            loggedIn = true;
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
        if (loggedIn == true)
        {
            loginScreen.SetActive(false);
            homeScreen.SetActive(true);
            loginEmailInputField.text = "";
            loginPasswordInputField.text = "";
        }
    }

    public void Logout()
    {
        Debug.Log("Log Out!");
        loggedIn = false;   
        auth.SignOut();
    }
    
    private bool isValidEmail(string email) {
        try {
            var addr = new MailAddress(email);
            return true;
        }
        catch {
            return false;
        }
    }

    private bool ValidatePassword(string password)
    {
        // Set the minimum password length
        int minLength = 8;

        // Check the password length
        if (password.Length < minLength)
            return false;

        // Check if the password contains at least one uppercase letter
        if (!Regex.IsMatch(password, "[A-Z]"))
            return false;

        // Check if the password contains at least one lowercase letter
        if (!Regex.IsMatch(password, "[a-z]"))
            return false;

        // Check if the password contains at least one digit
        if (!Regex.IsMatch(password, "[0-9]"))
            return false;
        
        return true;
    }

    private void WriteNewPlayer(string name, string email, string password, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int birdsSnapped, string[] achievements, int level)
    {
        PlayerData player = new PlayerData(name, email, password, creationDate, lastLoginDate, highScore, gamesPlayed, birdsSnapped, achievements, level);
        
        string json = JsonUtility.ToJson(player);
        Debug.Log(json);
        Debug.Log(user.UserId);
        reference.Child("players").Child(user.UserId).SetRawJsonValueAsync(json);
    }

    public void WriteNewScore(int score)
    {
        DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        dbReference.Child("players/" + user.UserId + "/score").SetValueAsync(score);
        DatabaseReference reference =
            FirebaseDatabase.DefaultInstance.GetReference("players/");
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates[user.UserId + "/score"] = score;
        reference.UpdateChildrenAsync(childUpdates);
    }
}
