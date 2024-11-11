using System;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Google.MiniJSON;
using TMPro;
using UnityEditor;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Firebase;
using Firebase.Extensions;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private int numberOfPlayers;
    
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject signupScreen;
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject resetPasswordScreen;
    
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
    
    //Reset Password
    [SerializeField] private TMP_InputField resetPasswordEmailInputField;
    [SerializeField] private TextMeshProUGUI resetPasswordValidationText;

    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        
        //NOTE: remove on build
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
        }

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
    
    //Account Interaction Functions

    public void SignUp()  
    {
        Debug.Log("Submit Values (Signup)");
        if (!string.IsNullOrWhiteSpace(signupUsernameInputField.text) && 
            !string.IsNullOrWhiteSpace(signupEmailInputField.text) && 
            !string.IsNullOrWhiteSpace(signupPasswordInputField.text) && 
            !string.IsNullOrWhiteSpace(signupConfirmPasswordInputField.text))
        {
            if (signupPasswordInputField.text == signupConfirmPasswordInputField.text)
            {
                string username = signupUsernameInputField.text;
                string email = signupEmailInputField.text;
                string password = signupPasswordInputField.text;
                
                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    signupValidationText.text = HandleAuthExceptions(task.Exception);

                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    signupScreen.SetActive(false);
                    homeScreen.SetActive(true);
                    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                        if (task.IsCanceled) {
                            Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted)
                        {
                            Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                            string errortext = HandleAuthExceptions(task.Exception).ToString();
                            return;
                        }
                        Firebase.Auth.AuthResult result = task.Result;
                        Debug.LogFormat("User signed in successfully: {0} ({1})",
                            result.User.DisplayName, result.User.UserId);
                        WriteNewPlayer(
                            result.User.UserId,
                            username,
                            email,
                            "",
                            ConvertNowToTimeStamp(),
                            0,
                            0,
                            0,
                            new string[] { "New Snapper" },
                            1
                        );
                        ResetFields();
                    });
                }

                // Firebase user has been created.
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("auth user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
                });
            }
            else
            {
                signupValidationText.text = "Passwords do not match";
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
        if (!string.IsNullOrWhiteSpace(loginEmailInputField.text) &&
            !string.IsNullOrWhiteSpace(loginPasswordInputField.text))
        {
            auth.SignInWithEmailAndPasswordAsync(loginEmailInputField.text, loginPasswordInputField.text)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        loginValidationText.text = HandleAuthExceptions(task.Exception);
                        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    if (task.IsCompleted)
                    {
                        loginScreen.SetActive(false);
                        homeScreen.SetActive(true);
                        ResetFields();
                    }

                    Firebase.Auth.AuthResult result = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                });
        }
        else
        {
            loginValidationText.text = "Please fill all fields";
        }
    }

    public void ResetPassword()
    {
        if (!string.IsNullOrWhiteSpace(resetPasswordEmailInputField.text))
        {
            auth.SendPasswordResetEmailAsync(resetPasswordEmailInputField.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    resetPasswordValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    loginScreen.SetActive(true);
                    resetPasswordScreen.SetActive(false);
                    ResetFields();
                }

                Debug.Log("Password reset email sent successfully.");
            });
        }
        else
        {
            resetPasswordValidationText.text = "Please fill all fields";
        }
    }

    public void ResetFields()
    {
        signupUsernameInputField.text = "";
        signupEmailInputField.text = "";
        signupPasswordInputField.text = "";
        signupConfirmPasswordInputField.text = "";
        signupValidationText.text = "";
        loginEmailInputField.text = "";
        loginPasswordInputField.text = "";
        loginValidationText.text = "";
        resetPasswordValidationText.text = "";
        resetPasswordEmailInputField.text = "";
    }
    
    
    //Account Validation Functions
    public string HandleAuthExceptions(System.AggregateException e)
    {
        string validationText = "";

        if (e != null)
        {
            FirebaseException firebaseEx = e.GetBaseException() as FirebaseException;

            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            Debug.LogError("Error in auth.... error code: " + errorCode);

            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    validationText += "Wrong Password";
                    break;
                case AuthError.UserNotFound:
                    validationText += "User does not exist, please create an account";
                    break;
                case AuthError.InvalidEmail:
                    validationText += "Invalid Email";
                    break;
                case AuthError.WeakPassword:
                    validationText += "Weak Password (Minimum 8 Characters, requires upper case letters, lower case letters, numbers)";
                    break;
                case AuthError.EmailAlreadyInUse:
                    validationText += "Email is already in use, try logging in";
                    break;
                case AuthError.UserMismatch:
                    validationText += "User Mismatch";
                    break;
                case AuthError.Failure:
                    validationText += "Failed to login...";
                    break;
                default:
                    validationText += "Issue in authentication: " + errorCode;
                    break;
            }
        }
        return validationText;
    }
    
    //Db Writing Functions
    private void WriteNewPlayer(string uid, string name, string email, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int birdsSnapped, string[] achievements, float accuracy)
    {
        PlayerData player = new PlayerData(name, email, creationDate, lastLoginDate, highScore, gamesPlayed, birdsSnapped, achievements, accuracy);
        
        string json = JsonUtility.ToJson(player);
        Debug.Log(json);
        reference.Child("players").Child(uid).SetRawJsonValueAsync(json);
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
    
    public string ConvertNowToTimeStamp()
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        // Get the unix timestamp in seconds
        return dto.ToUnixTimeSeconds().ToString();
    }
}
