using GlitchHunter.Constant;
using GlitchHunter.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GlitchHunter.Handler.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsJumpInput { get; private set; }
        public bool IsSprintInput { get; private set; }
        public bool IsControllerSwitch { get; private set; }
        public bool IsShootingInput { get; private set; }
        public bool IsReloadInput { get; private set; }
        public bool IsZoomInput { get; private set; }

        private void Update()
        {
            // Shooting input
            IsShootingInput = Input.GetKey(KeyCode.Mouse0);
            GlitchHunterConstant.OnShootingInput?.Invoke(IsShootingInput);

            // Process movement input
            MoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            GlitchHunterConstant.OnMoveInput?.Invoke(MoveInput);

            // Process look input
            LookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            GlitchHunterConstant.OnLookInput?.Invoke(LookInput);

            // Jump/Fly input
            IsJumpInput = Input.GetKey(KeyCode.Space);
            GlitchHunterConstant.OnJumpInput?.Invoke(IsJumpInput);

            // Sprint input 
            IsSprintInput = Input.GetKey(KeyCode.LeftShift);
            GlitchHunterConstant.OnSprintInput?.Invoke(IsSprintInput);

            // Controller Switch Input 
            if (Input.GetKeyDown(KeyCode.X))
            {
                IsControllerSwitch = !IsControllerSwitch;
                GlitchHunterConstant.CurrentControllerType = IsControllerSwitch ? Enum.ControllerType.FIRST_PERSON : Enum.ControllerType.THIRD_PERSON;
                GlitchHunterConstant.OnSwitchController?.Invoke();
            }

            // Reload input
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GameManager.Instance.IsMeleeCombatStarted)
                {
                    return;
                }
                IsReloadInput = true;
                GlitchHunterConstant.OnWeaponReloadInput?.Invoke(IsReloadInput);
            }
            else
            {
                if (GameManager.Instance.IsMeleeCombatStarted)
                {
                    return;
                }
                IsReloadInput = false;
                GlitchHunterConstant.OnWeaponReloadInput?.Invoke(IsReloadInput);
            }

            //Zoom Input
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (GameManager.Instance.IsMeleeCombatStarted)
                {
                    return;
                }
                IsZoomInput = true;
                GlitchHunterConstant.OnZoomInput?.Invoke(IsZoomInput);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                if (GameManager.Instance.IsMeleeCombatStarted)
                {
                    return;
                }
                IsZoomInput = false;
                GlitchHunterConstant.OnZoomInput?.Invoke(IsZoomInput);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            GlitchHunterConstant.SetCursorState(cursorLocked);
        }
    }
}