using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;

    // Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;

    // Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    // Register variables
    [Header("Register")]
    public TMP_InputField usernameSignUpField;
    public TMP_InputField emailSignUpField;
    public TMP_InputField passwordSignUpField;
    public TMP_InputField confirmPasswordField;
    public TextMeshProUGUI warningText;

    public GameObject verifyEmail;
    public TextMeshProUGUI logText;
    public GameObject successUI, signUpUI, signInUI;
    public GameObject DashboardPanel, verifyButton;
    public TextMeshProUGUI userName;
    public TMP_InputField ResetEmailField;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // If they are available, initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        // Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton()
    {
        StartCoroutine(Login());
        PlayerPrefs.SetInt("player", 1);
    }

    public void RegisterButton()
    {
        PlayerPrefs.SetInt("player", 1);
        // Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailSignUpField.text, passwordSignUpField.text, usernameSignUpField.text));
    }

    IEnumerator Login()
    {
        string email = emailLoginField.text;
        string password = passwordLoginField.text;

        // Call the Firebase auth signin function passing the email and password
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        // Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            // If there are errors handle them
            Debug.LogWarning(message: $"Failed to login task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningText.text = message;
        }
        else
        {
            // User is now logged in
            // Now get the result
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);

            if (User.IsEmailVerified)
            {
                ShowLogMessage("Log in Successful");
                successUI.SetActive(true);
                signInUI.SetActive(false);
                StartCoroutine(ContinueToDashboard());
                successUI.transform.Find("id").GetComponent<TextMeshProUGUI>().text = "Id: " + User.UserId;
            }
            else
            {
                verifyEmail.SetActive(true);
                ShowLogMessage("Please verify email!!");
                signInUI.SetActive(false);
            }
        }
    }

    private IEnumerator Register(string email, string password, string username)
    {
        Debug.Log("Starting Register coroutine");

        if (username == "")
        {
            // If the username field is blank show a warning
            warningText.text = "Missing Username";
        }
        else if (passwordSignUpField.text != confirmPasswordField.text)
        {
            // If the password does not match show a warning
            warningText.text = "Password Does Not Match!";
        }
        else
        {
            // Ensure auth is initialized
            if (auth == null)
            {
                Debug.LogError("FirebaseAuth instance is null");
                yield break;
            }

            // Call the Firebase auth signin function passing the email and password
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            // Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningText.text = message;
                usernameSignUpField.text = string.Empty;
                emailSignUpField.text = string.Empty;
                passwordSignUpField.text = string.Empty;
                confirmPasswordField.text = string.Empty;
            }
            else
            {
                // User has now been created
                // Now get the result
                User = RegisterTask.Result.User;
                Debug.LogFormat("User created successfully: {0} ({1})", User.DisplayName, User.Email);

                if (User != null)
                {
                    // Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = username };

                    // Call the Firebase auth update user profile function passing the profile with the username
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    // Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningText.text = "Username Set Failed!";
                    }
                    else
                    {
                        // Username is now set
                        // Now return to login screen
                        warningText.text = string.Empty;
                    }

                    // Send email verification if not already verified
                    if (!User.IsEmailVerified)
                    {
                        signUpUI.SetActive(false);
                        ShowLogMessage("Please verify your email!!");
                        verifyEmail.SetActive(true);
                        SendEmailVerification();
                    }
                }
            }
        }
    }

    public void SendEmailVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    IEnumerator SendEmailForVerificationAsync()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                Debug.LogWarning("Failed to send verification email");
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                switch (error)
                {
                    // Handle specific errors if needed
                    default:
                        Debug.LogError("Email send error: " + error.ToString());
                        break;
                }
            }
            else
            {
                Debug.Log("Verification email sent successfully");
            }
        }
    }
    public void ResetPassword()
    {
        StartCoroutine(ForgotPassword(ResetEmailField.text));
    }
    private IEnumerator ForgotPassword(string Email)
    {
        if (string.IsNullOrEmpty(Email))
        {
            warningText.text = "missing Email";
            yield break;
        }
        var ResetTask = auth.SendPasswordResetEmailAsync(Email);
        yield return new WaitUntil(() => ResetTask.IsCompleted);

        if (ResetTask.Exception != null)
        {
            Debug.LogWarning(" Send Email for Reset password Failed");
            FirebaseException firebaseException = ResetTask.Exception.GetBaseException() as FirebaseException;
            AuthError error = (AuthError)firebaseException.ErrorCode;
            switch (error)
            {
                case AuthError.InvalidRecipientEmail:
                    warningText.text = "Invalid Email";
                    break;
                default:
                    Debug.Log("Reset Password Send Error"+error.ToString()); break;
            }
        }
        else
        {
            Debug.Log("Email Sent successfully for Password Reset");
        }
    }

    void ShowLogMessage(string msg)
    {
        logText.text = msg;
    }

    public void VerifyEmail()
    {
        Application.OpenURL("https://mail.google.com/mail/u/0/#inbox");
        passwordLoginField.text = string.Empty;
        verifyEmail.SetActive(false);
        StartCoroutine(LoginFromVerify());
    }

    IEnumerator ContinueToDashboard()
    {
        yield return new WaitForSeconds(1.5f);
        DashboardPanel.SetActive(true);
        successUI.SetActive(false);
        userName.text = User.DisplayName;
    }

    IEnumerator LoginFromVerify()
    {
        yield return new WaitForSeconds(1f);
        signInUI.SetActive(true);
    }

    public void ClearAllFields()
    {
        usernameSignUpField.text = string.Empty;
        passwordSignUpField.text = string.Empty;
        confirmPasswordField.text = string.Empty;
        emailLoginField.text = string.Empty;
        passwordLoginField.text = string.Empty;
        emailSignUpField.text = string.Empty;
    }
}
