using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MyUnityEngine.UI {
    /// <summary>
    /// Editable text input field.
    /// </summary>

    
    [AddComponentMenu("MyUI/Input Field", 31)] public class InputField
        : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ICanvasElement
    {
        // Setting the content type acts as a shortcut for setting a combination of InputType, CharacterValidation, LineType, and TouchScreenKeyboardType
        public enum ContentType {
            Standard
        }

        public enum InputType {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation {
            None,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            EmailAddress
        }

        public enum LineType {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        
        [Serializable] public class SubmitEvent : UnityEvent<string> {}

        [Serializable] public class OnChangeEvent : UnityEvent<string> {}

        protected TouchScreenKeyboard m_Keyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

        #region Exposed properties
        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary> 
        [SerializeField] [FormerlySerializedAs("text")] protected Text m_TextComponent;   

        [SerializeField] private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [FormerlySerializedAs("inputType")]
        [SerializeField]
        private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [FormerlySerializedAs("asteriskChar")]
        [SerializeField]
        private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [FormerlySerializedAs("keyboardType")]
        [SerializeField]
        private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input.
        /// </summary>

        [FormerlySerializedAs("hideMobileInput")]
        [SerializeField]
        private bool m_HideMobileInput = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [FormerlySerializedAs("validation")]
        [SerializeField]
        private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [FormerlySerializedAs("characterLimit")]
        [SerializeField]
        private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [FormerlySerializedAs("onSubmit")]
        [FormerlySerializedAs("m_OnSubmit")]
        [FormerlySerializedAs("m_EndEdit")]
        [SerializeField]
        private SubmitEvent m_OnEndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [FormerlySerializedAs("onValueChange")]
        [FormerlySerializedAs("m_OnValueChange")]
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [SerializeField] [FormerlySerializedAs("onValidateInput")] private OnValidateInput m_OnValidateInput;

        
        [SerializeField] private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        
        [SerializeField] private bool m_CustomCaretColor = false;

        
        
        [SerializeField] [FormerlySerializedAs("selectionColor")] private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>
        [SerializeField] [FormerlySerializedAs("mValue")] protected string m_VisibleContentsText = "a here comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\n"
                 + "here comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\nhere comes\n";
        
        [SerializeField] [Range(0f, 4f)] private float m_CaretBlinkRate = 0.85f;

        [SerializeField] [Range(1, 5)] private int m_CaretWidth = 1;

        #endregion

        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        private TextGenerator m_InputTextCache;
        private CanvasRenderer m_CachedInputRenderer;
        private bool m_PreventFontCallback = false;
        [NonSerialized] protected Mesh m_Mesh;
        private bool m_AllowInput = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        protected int m_DrawStart = 0;
        protected int m_DrawEnd = 0;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";

        protected Mesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        protected TextGenerator cachedInputTextGenerator
        {
            get
            {
                if (m_InputTextCache == null)
                    m_InputTextCache = new TextGenerator();

                return m_InputTextCache;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>

        public bool shouldHideMobileInput
        {
            set
            {
                SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
            }
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.TizenPlayer:
                        return m_HideMobileInput;
                }

                return true;
            }
        }

        public bool isFocused {
            get { return m_AllowInput; }
        }

        
        public Text textComponent { get { return m_TextComponent; } set { SetPropertyUtility.SetClass(ref m_TextComponent, value); } }

        public Color caretColor { get { return customCaretColor ? m_CaretColor : textComponent.color; } set { if (SetPropertyUtility.SetColor(ref m_CaretColor, value)) MarkGeometryAsDirty(); } }

        public bool customCaretColor { get { return m_CustomCaretColor; } set { if (m_CustomCaretColor != value) { m_CustomCaretColor = value; MarkGeometryAsDirty(); } } }

        public Color selectionColor { get { return m_SelectionColor; } set { if (SetPropertyUtility.SetColor(ref m_SelectionColor, value)) MarkGeometryAsDirty(); } }

        public SubmitEvent onEndEdit { get { return m_OnEndEdit; } set { SetPropertyUtility.SetClass(ref m_OnEndEdit, value); } }

        public OnChangeEvent onValueChanged { get { return m_OnValueChanged; } set { SetPropertyUtility.SetClass(ref m_OnValueChanged, value); } }

        // Content Type related

        public ContentType contentType { get { return m_ContentType; } set { if (SetPropertyUtility.SetStruct(ref m_ContentType, value)) EnforceContentType(); } }


        public InputType inputType { get { return m_InputType; } set { if (SetPropertyUtility.SetStruct(ref m_InputType, value)) SetToCustom(); } }

        public TouchScreenKeyboardType keyboardType { get { return m_KeyboardType; } set { if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value)) SetToCustom(); } }

        public CharacterValidation characterValidation { get { return m_CharacterValidation; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value)) SetToCustom(); } }

        
        // Derived property
       
        // Not shown in Inspector.
        public char asteriskChar { get { return m_AsteriskChar; } set { if (SetPropertyUtility.SetStruct(ref m_AsteriskChar, value)) UpdateLabel(); } }
        
        protected void ClampPos(ref int pos) {
            if (pos < 0) pos = 0;
            else if (pos > m_VisibleContentsText.Length) pos = m_VisibleContentsText.Length;
        }

        /// <summary>
        /// Current position of the cursor.
        /// Getters are public Setters are protected
        /// </summary>

        protected int caretPositionInternal { get { return m_CaretPosition + Input.compositionString.Length; } set { m_CaretPosition = value; ClampPos(ref m_CaretPosition); } }
        protected int caretSelectPositionInternal {
            get {
                return m_CaretSelectPosition + Input.compositionString.Length;
            }
            set {
                m_CaretSelectPosition = value; ClampPos(ref m_CaretSelectPosition);
            }
        }

        private bool hasSelection { get { return caretPositionInternal != caretSelectPositionInternal; } }

	#if UNITY_EDITOR
        [Obsolete("caretSelectPosition has been deprecated. Use selectionFocusPosition instead (UnityUpgradable) -> selectionFocusPosition", true)]
        public int caretSelectPosition { get { return selectionFocusPosition; } protected set { selectionFocusPosition = value; } }
	#endif

        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>

        public int caretPosition
        {
            get { return m_CaretSelectPosition + Input.compositionString.Length; }
            set { selectionAnchorPosition = value; selectionFocusPosition = value; }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If Input.compositionString is 0 set the fixed position
        /// </summary>

        public int selectionAnchorPosition
        {
            get { return m_CaretPosition + Input.compositionString.Length; }
            set
            {
                if (Input.compositionString.Length != 0)
                    return;

                m_CaretPosition = value;
                ClampPos(ref m_CaretPosition);
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If Input.compositionString is 0 set the variable position
        /// </summary>

        public int selectionFocusPosition
        {
			
            get {
				Debug.LogError("selectionFocusPosition!"); 
				return m_CaretSelectPosition + Input.compositionString.Length;
			}
            set
            {
                if (Input.compositionString.Length != 0) return;

                m_CaretSelectPosition = value;
                ClampPos(ref m_CaretSelectPosition);
            }
        }

    #if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate() {
            base.OnValidate();
            EnforceContentType();

            m_CharacterLimit = Math.Max(0, m_CharacterLimit);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive()) return;

            UpdateLabel();
            if (m_AllowInput) SetCaretActive();
        }

    #endif // if UNITY_EDITOR

        protected override void OnEnable() {
            base.OnEnable();
            
            // この時点で、m_VisibleContentsTextにはなんかが入ってる。ここで入れてもいい。というかここで入れるかな。
            // やりたいことは、文字の描画にまぎれこんで、コントロール可能なボタンを埋め込むこと。
            // そのためには、文字の描画周りに踏み込む必要がある。
            
            m_DrawStart = 0;
            m_DrawEnd = m_VisibleContentsText.Length;

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (m_CachedInputRenderer != null) m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
			
			if (m_TextComponent != null)
            {
				Debug.Log("OnEnable2");
                m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
                UpdateLabel();
            } else {
				Debug.Log("or OnEnable3");
			}
        }

        protected override void OnDisable() {
            // the coroutine will be terminated, so this will ensure it restarts when we are next activated
            m_BlinkCoroutine = null;

            DeactivateInputField();
            if (m_TextComponent != null) {
                m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
            }

            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            // Clear needs to be called otherwise sync never happens as the object is disabled.
            if (m_CachedInputRenderer != null) m_CachedInputRenderer.Clear();

            if (m_Mesh != null) DestroyImmediate(m_Mesh);

            m_Mesh = null;

            base.OnDisable();
        }

        IEnumerator CaretBlink() {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while (isFocused && m_CaretBlinkRate > 0) {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (m_CaretVisible != blinkState) {
                    m_CaretVisible = blinkState;
                    if (!hasSelection) MarkGeometryAsDirty();
                }

                // Then wait again.
                yield return null;
            }
            m_BlinkCoroutine = null;
        }

        void SetCaretVisible() {
            if (!m_AllowInput) return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f)
            {
                if (m_BlinkCoroutine == null)
                    m_BlinkCoroutine = StartCoroutine(CaretBlink());
            }
            else
            {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus() {
            Debug.LogError("テキストにフォーカス変わった時の処理、なんかあれば。以前選んでた箇所に再度キャレットを表示とかしたいね〜〜 m_CaretSelectPosition:" + m_CaretSelectPosition);
            // SelectAll();
        }

        protected void SelectAll() {
            caretPositionInternal = m_VisibleContentsText.Length;
            caretSelectPositionInternal = 0;
        }

        public void MoveTextEnd(bool shift)
        {
            int position = m_VisibleContentsText.Length;

            if (shift)
            {
                caretSelectPositionInternal = position;
            }
            else
            {
                caretPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal;
            }
            UpdateLabel();
        }

        public void MoveTextStart(bool shift)
        {
            int position = 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
            }
            else
            {
                caretPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal;
            }

            UpdateLabel();
        }

        static string clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private bool InPlaceEditing()
        {
            return !TouchScreenKeyboard.isSupported;
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        /**
            入力周りの処理を行ってる。選択とか全部ここっぽい。
        */
        protected virtual void LateUpdate() {
            // Only activate if we are not already activated.
            if (m_ShouldActivateNextUpdate) {
                if (!isFocused) {
                    ActivateInputFieldInternal();
                    m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                m_ShouldActivateNextUpdate = false;
            }

            if (InPlaceEditing() || !isFocused) return;


            AssignPositioningIfNeeded();

            if (m_Keyboard == null || !m_Keyboard.active) {
                if (m_Keyboard != null) {
                    if (m_Keyboard.wasCanceled) m_WasCanceled = true;
                }

                OnDeselect(null);
                return;
            }

            string val = m_Keyboard.text;

            if (m_VisibleContentsText != val) {
                Debug.LogError("一致しない");
                m_Keyboard.text = m_VisibleContentsText;
            } else {
                Debug.LogError("一致してる");
            }
            
            if (m_Keyboard.done) {
                if (m_Keyboard.wasCanceled) m_WasCanceled = true;
                OnDeselect(null);
            }
        }

        
        private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator) {
            // transform y to local scale
            float y = pos.y * m_TextComponent.pixelsPerUnit;
            float lastBottomY = 0.0f;

            for (int i = 0; i < generator.lineCount; ++i) {
                float topY = generator.lines[i].topY;
                float bottomY = topY - generator.lines[i].height;

                // pos is somewhere in the leading above this line
                if (y > topY) {
                    // determine which line we're closer to
                    float leading = topY - lastBottomY;
                    if (y > topY - 0.5f * leading) return i - 1;
                    else return i;
                }

                if (y > bottomY) return i;

                lastBottomY = bottomY;
            }

            // Position is after last line.
            return generator.lineCount;
        }

        /// <summary>
        /// Given an input position in local space on the Text return the index for the selection cursor at this position.
        /// </summary>

        protected int GetCharacterIndexFromPosition(Vector2 pos)
        {
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen.lineCount == 0) return 0;

            int line = GetUnclampedCharacterLineFromPosition(pos, gen);
            if (line < 0) return 0;
            if (line >= gen.lineCount) return gen.characterCountVisible;

            int startCharIndex = gen.lines[line].startCharIdx;
            int endCharIndex = GetLineEndPosition(gen, line);

            for (int i = startCharIndex; i < endCharIndex; i++) {
                if (i >= gen.characterCountVisible) break;

                UICharInfo charInfo = gen.characters[i];
                Vector2 charPos = charInfo.cursorPos / m_TextComponent.pixelsPerUnit;

                float distToCharStart = pos.x - charPos.x;
                float distToCharEnd = charPos.x + (charInfo.charWidth / m_TextComponent.pixelsPerUnit) - pos.x;
                if (distToCharStart < distToCharEnd) return i;
            }

            return endCharIndex;
        }

        private bool MayDrag(PointerEventData eventData) {
            return IsActive() &&
                   IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   m_TextComponent != null &&
                   m_Keyboard == null;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) return;

            m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) return;
            
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);
            caretSelectPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;// ここがドラッグでのスクロールの処理っぽいな。
            MarkGeometryAsDirty();

            m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textComponent.rectTransform, eventData.position, eventData.pressEventCamera);
            if (m_DragPositionOutOfBounds && m_DragCoroutine == null) m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

            eventData.Use();
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData) {
            while (m_UpdateDrag && m_DragPositionOutOfBounds) {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

                Rect rect = textComponent.rectTransform.rect;
                
                if (localMousePos.y > rect.yMax) MoveUp(true, true);
                else if (localMousePos.y < rect.yMin) MoveDown(true, true);

                UpdateLabel();
                
                yield return null;
            }
            m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (!InPlaceEditing())
            {
                if (m_Keyboard == null || !m_Keyboard.active)
                {
                    OnSelect(eventData);
                    return;
                }
            }

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore)
            {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

                caretSelectPositionInternal = caretPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;
            }
            UpdateLabel();
            eventData.Use();
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                {
                    Backspace();
                    return EditState.Continue;
                }

                case KeyCode.Delete:
                {
                    ForwardSpace();
                    return EditState.Continue;
                }

                case KeyCode.Home:
                {
                    MoveTextStart(shift);
                    return EditState.Continue;
                }

                case KeyCode.End:
                {
                    MoveTextEnd(shift);
                    return EditState.Continue;
                }

                // Select All
                case KeyCode.A:
                {
                    if (ctrlOnly)
                    {
                        SelectAll();
                        return EditState.Continue;
                    }
                    break;
                }

                // Copy
                case KeyCode.C:
                {
                    if (ctrlOnly)
                    {
                        if (inputType != InputType.Password)
                            clipboard = GetSelectedString();
                        else
                            clipboard = "";
                        return EditState.Continue;
                    }
                    break;
                }

                // Paste
                case KeyCode.V:
                {
                    if (ctrlOnly)
                    {
                        Append(clipboard);
                        return EditState.Continue;
                    }
                    break;
                }

                // Cut
                case KeyCode.X:
                {
                    if (ctrlOnly)
                    {
                        if (inputType != InputType.Password)
                            clipboard = GetSelectedString();
                        else
                            clipboard = "";
                        Delete();
                        SendOnValueChangedAndUpdateLabel();
                        return EditState.Continue;
                    }
                    break;
                }

                case KeyCode.LeftArrow:
                {
                    MoveLeft(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.RightArrow:
                {
                    MoveRight(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.UpArrow:
                {
                    MoveUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.DownArrow:
                {
                    MoveDown(shift);
                    return EditState.Continue;
                }

                // // Submit
                // case KeyCode.Return:
                // case KeyCode.KeypadEnter:
                // {
                //     if (lineType != LineType.MultiLineNewline)
                //     {
                //         return EditState.Finish;
                //     }
                //     break;
                // }

                case KeyCode.Escape:
                {
                    m_WasCanceled = true;
                    return EditState.Finish;
                }
            }

            char c = evt.character;
           
            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            if (IsValidChar(c))
            {
                Append(c);
            }

            if (c == 0)
            {
                if (Input.compositionString.Length > 0)
                {
                    UpdateLabel();
                }
            }
            return EditState.Continue;
        }

        private bool IsValidChar(char c)
        {
            // Delete key on mac
            if ((int)c == 127)
                return false;
            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            return m_TextComponent.font.HasCharacter(c);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            KeyPressed(e);
        }

        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    consumedEvent = true;
                    var shouldContinue = KeyPressed(m_ProcessingEvent);
                    if (shouldContinue == EditState.Finish)
                    {
                        DeactivateInputField();
                        break;
                    }
                }

                switch (m_ProcessingEvent.type)
                {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (m_ProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            if (consumedEvent)
                UpdateLabel();

            eventData.Use();
        }

        private string GetSelectedString() {
            Debug.LogError("大変美味しそうな関数");
            if (!hasSelection) return "";

            int startPos = caretPositionInternal;
            int endPos = caretSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos) {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            return m_VisibleContentsText.Substring(startPos, endPos - startPos);
        }

        private int FindtNextWordBegin()
        {
            if (caretSelectPositionInternal + 1 >= m_VisibleContentsText.Length) return m_VisibleContentsText.Length;

            int spaceLoc = m_VisibleContentsText.IndexOfAny(kSeparators, caretSelectPositionInternal + 1);

            if (spaceLoc == -1) spaceLoc = m_VisibleContentsText.Length;
            else spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
                return;
            }

            int position;
            if (ctrl)
                position = FindtNextWordBegin();
            else
                position = caretSelectPositionInternal + 1;

            if (shift)
                caretSelectPositionInternal = position;
            else
                caretSelectPositionInternal = caretPositionInternal = position;
        }

        private int FindtPrevWordBegin()
        {
            if (caretSelectPositionInternal - 2 < 0)
                return 0;

            int spaceLoc = m_VisibleContentsText.LastIndexOfAny(kSeparators, caretSelectPositionInternal - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
                return;
            }

            int position;
            if (ctrl)
                position = FindtPrevWordBegin();
            else
                position = caretSelectPositionInternal - 1;

            if (shift)
                caretSelectPositionInternal = position;
            else
                caretSelectPositionInternal = caretPositionInternal = position;
        }

        private int DetermineCharacterLine(int charPos, TextGenerator generator)
        {
            for (int i = 0; i < generator.lineCount - 1; ++i)
            {
                if (generator.lines[i + 1].startCharIdx > charPos)
                    return i;
            }

            return generator.lineCount - 1;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos > cachedInputTextGenerator.characterCountVisible)
                return 0;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the first line return first character
            if (originLine <= 0)
                return goToFirstChar ? 0 : originalPos;

            int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

            for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= cachedInputTextGenerator.characterCountVisible)
                return m_VisibleContentsText.Length;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine + 1 >= cachedInputTextGenerator.lineCount)
                return goToLastChar ? m_VisibleContentsText.Length : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);

            for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        private void MoveDown(bool shift)
        {
            MoveDown(shift, true);
        }

        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret position to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar);

            if (shift) caretSelectPositionInternal = position;
            else caretPositionInternal = caretSelectPositionInternal = position;
        }

        private void MoveUp(bool shift)
        {
            MoveUp(shift, true);
        }

        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar);

            if (shift) caretSelectPositionInternal = position;
            else caretSelectPositionInternal = caretPositionInternal = position;
        }

        private void Delete() {
            Debug.LogError("do nothing.");
        }

        private void ForwardSpace() {
            Debug.LogError("do nothing.");
        }

        private void Backspace() {
            Debug.LogError("do nothing.");
        }

        // Insert the character and update the label.
        private void Insert(char c) {
            Debug.LogError("do nothing.");
        }

        private void SendOnValueChangedAndUpdateLabel() {
			Debug.LogError("SendOnValueChangedAndUpdateLabel");
            SendOnValueChanged();
            UpdateLabel();
        }

        private void SendOnValueChanged() {
            if (onValueChanged != null)
                onValueChanged.Invoke(m_VisibleContentsText);
        }

        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>

        protected virtual void Append(string input) {
            Debug.LogError("do nothing.");
        }

        protected virtual void Append(char input) {
            Debug.LogError("do nothing.");
        }

		/*
			m_CaretPositionに基づいて文字が移動してるんで、これがどう動くかを制御できればスクロールできそう。
		*/

        /// <summary>
        /// Update the visual text Text.
        /// </summary>
        protected void UpdateLabel() {
            if (m_TextComponent != null && m_TextComponent.font != null && !m_PreventFontCallback) {
                // TextGenerator.Populate invokes a callback that's called for anything
                // that needs to be updated when the data for that font has changed.
                // This makes all Text components that use that font update their vertices.
                // In turn, this makes the InputField that's associated with that Text component
                // update its label by calling this UpdateLabel method.
                // This is a recursive call we want to prevent, since it makes the InputField
                // update based on font data that didn't yet finish executing, or alternatively
                // hang on infinite recursion, depending on whether the cached value is cached
                // before or after the calculation.
                //
                // This callback also occurs when assigning text to our Text component, i.e.,
                // m_TextComponent.text = processed;

                m_PreventFontCallback = true;

                var fullText = string.Empty;
                if (Input.compositionString.Length > 0) fullText = m_VisibleContentsText.Substring(0, m_CaretPosition) + Input.compositionString + m_VisibleContentsText.Substring(m_CaretPosition);
                else fullText = m_VisibleContentsText;

                var processed = string.Empty;
                if (inputType == InputType.Password) processed = new string(asteriskChar, fullText.Length);
                else processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.
                if (!m_AllowInput) {
                    m_DrawStart = 0;
                    m_DrawEnd = m_VisibleContentsText.Length;
                }

                if (!isEmpty) {
                    // Determine what will actually fit into the given line
                    Vector2 extents = m_TextComponent.rectTransform.rect.size;

                    var settings = m_TextComponent.GetGenerationSettings(extents);
                    settings.generateOutOfBounds = true;

                    cachedInputTextGenerator.Populate(processed, settings);

                    SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);

                    processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

                    SetCaretVisible();
                }

                m_TextComponent.text = processed;
				// Debug.LogError("これかな m_TextComponent.text:" + m_TextComponent.text);

                MarkGeometryAsDirty();
                m_PreventFontCallback = false;
            }
        }

        private bool IsSelectionVisible() {
			Debug.LogError("IsSelectionVisible");
            if (m_DrawStart > caretPositionInternal || m_DrawStart > caretSelectPositionInternal) return false;
            if (m_DrawEnd < caretPositionInternal || m_DrawEnd < caretSelectPositionInternal) return false;
            return true;
        }

        private static int GetLineStartPosition(TextGenerator gen, int line)
        {
            line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
            return gen.lines[line].startCharIdx;
        }

        private static int GetLineEndPosition(TextGenerator gen, int line)
        {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count)
                return gen.lines[line + 1].startCharIdx - 1;
            return gen.characterCountVisible;
        }

        private void SetDrawRangeToContainCaretPosition(int caretPos)
        {
            // We don't have any generated lines generation is not valid.
            if (cachedInputTextGenerator.lineCount <= 0)
                return;

            // the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
            // the values returned by the TextGenerator.lines[x].height for instance.
            Vector2 extents = cachedInputTextGenerator.rectExtents.size;
            
            var lines = cachedInputTextGenerator.lines;
            int caretLine = DetermineCharacterLine(caretPos, cachedInputTextGenerator);

            if (caretPos > m_DrawEnd)
            {
                // Caret comes after drawEnd, so we need to move drawEnd to the end of the line with the caret
                m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, caretLine);
                float bottomY = lines[caretLine].topY - lines[caretLine].height;
                int startLine = caretLine;
                while (startLine > 0)
                {
                    float topY = lines[startLine - 1].topY;
                    if (topY - bottomY > extents.y)
                        break;
                    startLine--;
                }
                m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            }
            else
            {
                if (caretPos < m_DrawStart)
                {
                    // Caret comes before drawStart, so we need to move drawStart to an earlier line start that comes before caret.
                    m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, caretLine);
                }

                int startLine = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
                int endLine = startLine;

                float topY = lines[startLine].topY;
                float bottomY = lines[endLine].topY - lines[endLine].height;

                while (endLine < lines.Count - 1)
                {
                    bottomY = lines[endLine + 1].topY - lines[endLine + 1].height;
                    if (topY - bottomY > extents.y)
                        break;
                    ++endLine;
                }
                m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);

                while (startLine > 0)
                {
                    topY = lines[startLine - 1].topY;
                    if (topY - bottomY > extents.y)
                        break;
                    startLine--;
                }
                m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            }
        }

        public void ForceLabelUpdate() {
            UpdateLabel();
        }

        private void MarkGeometryAsDirty() {
	#if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null)
                return;
	#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update) {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete()
        {}

        public virtual void GraphicUpdateComplete()
        {}

        private void UpdateGeometry() {

            #if UNITY_EDITOR
                if (!Application.isPlaying) return;
            #endif

            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (!shouldHideMobileInput)
                return;

            if (m_CachedInputRenderer == null && m_TextComponent != null)
            {
                GameObject go = new GameObject(transform.name + " Input Caret");
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(m_TextComponent.transform.parent);
                go.transform.SetAsFirstSibling();
                go.layer = gameObject.layer;

                caretRectTrans = go.AddComponent<RectTransform>();
                m_CachedInputRenderer = go.AddComponent<CanvasRenderer>();
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

                // Needed as if any layout is present we want the caret to always be the same as the text area.
                go.AddComponent<LayoutElement>().ignoreLayout = true;

                AssignPositioningIfNeeded();
            }

            if (m_CachedInputRenderer == null)
                return;

            OnFillVBO(mesh);
            m_CachedInputRenderer.SetMesh(mesh);
        }

        private void AssignPositioningIfNeeded() {
            // 一瞬しか発生しない。生成系なのかな。
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
            {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
            }
        }

        private void OnFillVBO(Mesh vbo) {
            using (var helper = new VertexHelper())
            {
                if (!isFocused)
                {
                    helper.FillMesh(vbo);
                    return;
                }

                Rect inputRect = m_TextComponent.rectTransform.rect;
                Vector2 extents = inputRect.size;

                // get the text alignment anchor point for the text in local space
                Vector2 textAnchorPivot = Text.GetTextAnchorPivot(m_TextComponent.alignment);
                Vector2 refPoint = Vector2.zero;

                refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
                refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

                // Adjust the anchor point in screen space
                Vector2 roundedRefPoint = m_TextComponent.PixelAdjustPoint(refPoint);

                // Determine fraction of pixel to offset text mesh.
                // This is the rounding in screen space, plus the fraction of a pixel the text anchor pivot is from the corner of the text mesh.
                Vector2 roundingOffset = roundedRefPoint - refPoint + Vector2.Scale(extents, textAnchorPivot);
                roundingOffset.x = roundingOffset.x - Mathf.Floor(0.5f + roundingOffset.x);
                roundingOffset.y = roundingOffset.y - Mathf.Floor(0.5f + roundingOffset.y);

                if (!hasSelection)
                    GenerateCaret(helper, roundingOffset);
                else
                    GenerateHightlight(helper, roundingOffset);

                helper.FillMesh(vbo);
            }
        }

        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset) {
            if (!m_CaretVisible)
                return;

            if (m_CursorVerts == null)
            {
                CreateCursorVerts();
            }

            float width = m_CaretWidth;
            int adjustedPos = Mathf.Max(0, caretPositionInternal - m_DrawStart);
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen == null)
                return;

            if (gen.lineCount == 0)
                return;

            Vector2 startPosition = Vector2.zero;

            // Calculate startPosition
            if (adjustedPos < gen.characters.Count)
            {
                UICharInfo cursorChar = gen.characters[adjustedPos];
                startPosition.x = cursorChar.cursorPos.x;
            }
            startPosition.x /= m_TextComponent.pixelsPerUnit;

            // TODO: Only clamp when Text uses horizontal word wrap.
            if (startPosition.x > m_TextComponent.rectTransform.rect.xMax)
                startPosition.x = m_TextComponent.rectTransform.rect.xMax;

            int characterLine = DetermineCharacterLine(adjustedPos, gen);
            startPosition.y = gen.lines[characterLine].topY / m_TextComponent.pixelsPerUnit;
            float height = gen.lines[characterLine].height / m_TextComponent.pixelsPerUnit;

            for (int i = 0; i < m_CursorVerts.Length; i++)
                m_CursorVerts[i].color = caretColor;

            m_CursorVerts[0].position = new Vector3(startPosition.x, startPosition.y - height, 0.0f);
            m_CursorVerts[1].position = new Vector3(startPosition.x + width, startPosition.y - height, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, startPosition.y, 0.0f);
            m_CursorVerts[3].position = new Vector3(startPosition.x, startPosition.y, 0.0f);

            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < m_CursorVerts.Length; i++)
                {
                    UIVertex uiv = m_CursorVerts[i];
                    uiv.position.x += roundingOffset.x;
                    uiv.position.y += roundingOffset.y;
                }
            }

            vbo.AddUIVertexQuad(m_CursorVerts);

            int screenHeight = Screen.height;
            // Removed multiple display support until it supports none native resolutions(case 741751)
            //int displayIndex = m_TextComponent.canvas.targetDisplay;
            //if (Screen.fullScreen && displayIndex < Display.displays.Length)
            //    screenHeight = Display.displays[displayIndex].renderingHeight;

            startPosition.y = screenHeight - startPosition.y;
            Input.compositionCursorPos = startPosition;
        }

        private void CreateCursorVerts() {
            m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < m_CursorVerts.Length; i++) {
                m_CursorVerts[i] = UIVertex.simpleVert;
                m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }

        private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset) {
            Debug.LogError("選択範囲を作ってるぽいぞ");
            int startChar = Mathf.Max(0, caretPositionInternal - m_DrawStart);
            int endChar = Mathf.Max(0, caretSelectPositionInternal - m_DrawStart);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar) {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen.lineCount <= 0) return;

            int currentLineIndex = DetermineCharacterLine(startChar, gen);

            int lastCharInLineIndex = GetLineEndPosition(gen, currentLineIndex);

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < gen.characterCount) {
                if (currentChar == lastCharInLineIndex || currentChar == endChar)
                {
                    UICharInfo startCharInfo = gen.characters[startChar];
                    UICharInfo endCharInfo = gen.characters[currentChar];
                    Vector2 startPosition = new Vector2(startCharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit, gen.lines[currentLineIndex].topY / m_TextComponent.pixelsPerUnit);
                    Vector2 endPosition = new Vector2((endCharInfo.cursorPos.x + endCharInfo.charWidth) / m_TextComponent.pixelsPerUnit, startPosition.y - gen.lines[currentLineIndex].height / m_TextComponent.pixelsPerUnit);

                    // Checking xMin as well due to text generator not setting position if char is not rendered.
                    if (endPosition.x > m_TextComponent.rectTransform.rect.xMax || endPosition.x < m_TextComponent.rectTransform.rect.xMin)
                        endPosition.x = m_TextComponent.rectTransform.rect.xMax;

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    lastCharInLineIndex = GetLineEndPosition(gen, currentLineIndex);
                }
                currentChar++;
            }
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>

        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = caretPositionInternal == 0 || caretSelectPositionInternal == 0;
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;
                    if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Name)
            {
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // Space followed by a letter -- make sure it's capitalized
                    if (char.IsLower(ch) && lastChar == ' ')
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces (and apostrophes)
                    if (char.IsUpper(ch) && lastChar != ' ' && lastChar != '\'')
                        return char.ToLower(ch);

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }
                else if (ch == ' ')
                {
                    // Don't allow more than one space in a row
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            return (char)0;
        }

        public void ActivateInputField() {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable()) return;

            if (isFocused) {
                if (m_Keyboard != null && !m_Keyboard.active) {
                    m_Keyboard.active = true;
                    m_Keyboard.text = m_VisibleContentsText;
                    Debug.LogError("テキスト反映してる");
                }
            }
            
            m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal() {
            if (EventSystem.current == null) return;

            if (EventSystem.current.currentSelectedGameObject != gameObject) EventSystem.current.SetSelectedGameObject(gameObject);

            if (TouchScreenKeyboard.isSupported) {
                if (Input.touchSupported) {
                    TouchScreenKeyboard.hideInput = shouldHideMobileInput;
                }

                m_Keyboard = (inputType == InputType.Password) ?
                    TouchScreenKeyboard.Open(m_VisibleContentsText, keyboardType, false, true, true) :
                    TouchScreenKeyboard.Open(m_VisibleContentsText, keyboardType, inputType == InputType.AutoCorrect, true);

                // Mimics OnFocus but as mobile doesn't properly support select all
                // just set it to the end of the text (where it would move when typing starts)
                MoveTextEnd(false);
            } else {
                Input.imeCompositionMode = IMECompositionMode.On;
                OnFocus();
            }

            m_AllowInput = true;
            m_OriginalText = m_VisibleContentsText;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData) {
            Debug.LogError("おーークリックのたびに発生してる。");
            if (eventData.button != PointerEventData.InputButton.Left) return;

            ActivateInputField();
        }

        public void DeactivateInputField() {
            // Not activated do nothing.
            if (!m_AllowInput) return;

            m_HasDoneFocusTransition = false;
            m_AllowInput = false;

            if (m_TextComponent != null && IsInteractable()) {
                if (m_WasCanceled) m_VisibleContentsText = m_OriginalText;

                if (m_Keyboard != null) {
                    m_Keyboard.active = false;
                    m_Keyboard = null;
                }

                m_CaretPosition = m_CaretSelectPosition = 0;

                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData) {
            DeactivateInputField();
            base.OnDeselect(eventData);
        }

        private void EnforceContentType() {
            switch (contentType) {
                case ContentType.Standard: {
                    // Don't enforce line type for this content type.
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.Default;
                    m_CharacterValidation = CharacterValidation.None;
                    return;
                }
                // case ContentType.Autocorrected: {
                //     // Don't enforce line type for this content type.
                //     m_InputType = InputType.AutoCorrect;
                //     m_KeyboardType = TouchScreenKeyboardType.Default;
                //     m_CharacterValidation = CharacterValidation.None;
                //     return;
                // }
                // case ContentType.IntegerNumber: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Standard;
                //     m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                //     m_CharacterValidation = CharacterValidation.Integer;
                //     return;
                // }
                // case ContentType.DecimalNumber: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Standard;
                //     m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                //     m_CharacterValidation = CharacterValidation.Decimal;
                //     return;
                // }
                // case ContentType.Alphanumeric: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Standard;
                //     m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                //     m_CharacterValidation = CharacterValidation.Alphanumeric;
                //     return;
                // }
                // case ContentType.Name: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Standard;
                //     m_KeyboardType = TouchScreenKeyboardType.Default;
                //     m_CharacterValidation = CharacterValidation.Name;
                //     return;
                // }
                // case ContentType.EmailAddress: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Standard;
                //     m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                //     m_CharacterValidation = CharacterValidation.EmailAddress;
                //     return;
                // }
                // case ContentType.Password: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Password;
                //     m_KeyboardType = TouchScreenKeyboardType.Default;
                //     m_CharacterValidation = CharacterValidation.None;
                //     return;
                // }
                // case ContentType.Pin: {
                //     m_LineType = LineType.SingleLine;
                //     m_InputType = InputType.Password;
                //     m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                //     m_CharacterValidation = CharacterValidation.Integer;
                //     return;
                // }
                default:
                {
                    // Includes Custom type. Nothing should be enforced.
                    return;
                }
            }
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            // if (contentType == ContentType.Custom) return;

            for (int i = 0; i < allowedContentTypes.Length; i++) {
                if (contentType == allowedContentTypes[i]) return;
            }

            // contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            // if (contentType == ContentType.Custom)
            //     return;

            // contentType = ContentType.Custom;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (m_HasDoneFocusTransition) state = SelectionState.Highlighted;
            else if (state == SelectionState.Pressed) m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }
    }
}

namespace MyUnityEngine.UI
{
    internal static class SetPropertyUtility
    {
        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a) return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct {
            if (currentValue.Equals(newValue)) return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue))) return false;

            currentValue = newValue;
            return true;
        }
    }
}
