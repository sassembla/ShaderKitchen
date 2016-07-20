using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unidon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnidonUI.UI {
    /// <summary>
    /// Editable text input field.
    /// </summary>

    
    [AddComponentMenu("UnidonUI/SelectableTextField", 31)] public class SelectableTextField
        : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ICanvasElement
    {
        
        [SerializeField] public TextAsset textData;

        protected TouchScreenKeyboard m_Keyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

        private Text m_TextComponent;

        [SerializeField] private Font m_font;
        [SerializeField] private int m_fontSize;
        [SerializeField] private float m_fontLineSpacing;

        
        /// <summary>
        /// Should hide mobile input.
        /// </summary>
        [FormerlySerializedAs("hideMobileInput")]
        [SerializeField]
        private bool m_HideMobileInput = false;
        
        // /// <summary>
        // /// Event delegates triggered when the input field submits its data.
        // /// </summary>
        // [FormerlySerializedAs("onSubmit")]
        // [FormerlySerializedAs("m_OnSubmit")]
        // [FormerlySerializedAs("m_EndEdit")]
        // [SerializeField]
        // private SubmitEvent m_OnEndEdit = new SubmitEvent();// こういう書き方でハンドラ設定できるんで、イベントのidentityとアクションを外部に書けると思う。上に何か出す系。

        
        [SerializeField] private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        
        [SerializeField] private bool m_CustomCaretColor = false;
        
        [SerializeField] [FormerlySerializedAs("selectionColor")] private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        private string m_VisibleContentsText;
        private List<WebViewFunction.PointInfo> m_PointInfos;
        
        private float m_CaretBlinkRate = 0.85f;

        private int m_CaretWidth = 1;


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
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        protected int m_DrawStart = 0;
        protected int m_DrawEnd = 0;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;

        
        protected Mesh DrawingMesh {
            get {
                if (m_Mesh == null) m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        protected TextGenerator cachedInputTextGenerator {
            get {
                if (m_InputTextCache == null) m_InputTextCache = new TextGenerator();
                return m_InputTextCache;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>

        public bool shouldHideMobileInput {
            set {
                SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
            }
            get {
                switch (Application.platform) {
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





        // Derived property
       



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


        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>

        public int caretPosition {
            get {                
                return m_CaretSelectPosition + Input.compositionString.Length; }
            set { selectionAnchorPosition = value; selectionFocusPosition = value; }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If Input.compositionString is 0 set the fixed position
        /// </summary>

        public int selectionAnchorPosition {
            get {
                return m_CaretPosition + Input.compositionString.Length;
            }
            set {
                if (Input.compositionString.Length != 0) return;

                m_CaretPosition = value;
                ClampPos(ref m_CaretPosition);
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If Input.compositionString is 0 set the variable position
        /// </summary>

        public int selectionFocusPosition {
			
            get {
				return m_CaretSelectPosition + Input.compositionString.Length;
			}
            set {
                if (Input.compositionString.Length != 0) return;

                m_CaretSelectPosition = value;
                ClampPos(ref m_CaretSelectPosition);
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            
            var pointInfosAndRichText = WebViewFunction.MarkdownToRichText(textData.text);
            m_VisibleContentsText = pointInfosAndRichText.richText;
            m_PointInfos = pointInfosAndRichText.points;
            m_PointInfos.Reverse();


            m_DrawStart = 0;
            m_DrawEnd = m_VisibleContentsText.Length;

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (m_CachedInputRenderer != null) m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
			
            #if UNITY_EDITOR
                if (!Application.isPlaying) return;
            #endif

            // generate new text object.
            var textObj = new GameObject("New text object");
            m_TextComponent = textObj.AddComponent<Text>();
            textObj.transform.SetParent(this.transform);
            textObj.transform.localScale = new Vector3(1,1,1);

            // set anchor.
            var rt = textObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(-10, -10);
            
            // set font settings.
            m_TextComponent.font = m_font;
            m_TextComponent.fontSize = m_fontSize;
            m_TextComponent.lineSpacing = m_fontLineSpacing;
            

            if (m_TextComponent != null) {
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
        void SetCaretActive() {
            if (!m_AllowInput) return;

            if (m_CaretBlinkRate > 0.0f) {
                if (m_BlinkCoroutine == null) m_BlinkCoroutine = StartCoroutine(CaretBlink());
            } else {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus() {
            // SelectAll();
        }

        protected void SelectAll() {
            caretPositionInternal = m_VisibleContentsText.Length;
            caretSelectPositionInternal = 0;
        }

        public void MoveTextEnd(bool shift) {
            int position = m_VisibleContentsText.Length;

            if (shift) {
                caretSelectPositionInternal = position;
            } else {
                caretPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal;
            }
            UpdateLabel();
        }

        public void MoveTextStart(bool shift) {
            int position = 0;

            if (shift) {
                caretSelectPositionInternal = position;
            } else {
                caretPositionInternal = position;
                caretSelectPositionInternal = caretPositionInternal;
            }

            UpdateLabel();
        }

        static string clipboard {
            get {
                return GUIUtility.systemCopyBuffer;
            }
            set {
                // ignore html tags.
                var rgx = new Regex(@"<(.*?)>");
                var untagged = rgx.Replace(value, string.Empty);

                #if UNITY_WEBGL
                {
                    WebViewFunction.CopyToClipboard(untagged);
                }
                #else
                {
                    GUIUtility.systemCopyBuffer = untagged;
                }
                #endif
            }
        }

        private bool InPlaceEditing() {
            return !TouchScreenKeyboard.isSupported;
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        /**
            入力周りの処理を行ってる。選択とか全部ここっぽい。
            文字の描画はここではない。
        */
        protected virtual void LateUpdate() {
            // not yet work.
            var d = Input.GetAxis("Mouse ScrollWheel");
            if (d > 0f) {
                Debug.LogError("up");
            } else if (d < 0f) {
                Debug.LogError("down");
            }

            foreach (var item in m_PointInfos) {
                if (item.id != "Prop") continue; 
                // Debug.LogError("item.id:" + item.id);
                // Debug.LogError("item.index:" + item.index);
                var yStartPos = GetActualTextWritePixelPosByLineCount(item.index);


                // Debug.LogError("item.count:" + item.count);
                var yEndPos = GetActualTextWritePixelPosByLineCount(item.index + item.count);



                Debug.LogError("yStartPos:" + yStartPos);// これdrawした時のピクセル位置だ。UI上の位置じゃない。
                Debug.LogError("yEndPos:" + yEndPos);
            }

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

            // string val = m_Keyboard.text;
            // if (m_VisibleContentsText != val) {
            //     m_Keyboard.text = m_VisibleContentsText;
            // }

            
            
            
            if (m_Keyboard.done) {
                if (m_Keyboard.wasCanceled) m_WasCanceled = true;
                OnDeselect(null);
            }
        }

        /**
            実際の描画ピクセル位置を返す。
        */
        private float GetActualTextWritePixelPosByLineCount (int lineCount) {
            if (m_TextComponent == null) return 0;
            var gen = m_TextComponent.cachedTextGenerator;
            if (gen.lineCount == 0) return 0;
            if (gen.lines.Count <= lineCount) return 0;
            return gen.lines[lineCount].topY;
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
        protected int GetCharacterIndexFromPosition(Vector2 pos) {
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
            caretSelectPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;
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

        public virtual void OnEndDrag(PointerEventData eventData) {
            if (!MayDrag(eventData)) return;
            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (!MayDrag(eventData)) return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (!InPlaceEditing()) {
                if (m_Keyboard == null || !m_Keyboard.active) {
                    OnSelect(eventData);
                    return;
                }
            }

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore) {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

                caretSelectPositionInternal = caretPositionInternal = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;
            }
            UpdateLabel();
            eventData.Use();
        }

        protected void KeyPressed(Event evt) {
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;
            
            // Debug.LogError("evt:" + evt);

            // command
            if (evt.character == '\0') {
                switch (evt.keyCode) {
                    // Select All
                    case KeyCode.A: {
                        if (ctrlOnly) {
                            SelectAll();
                            return;
                        }
                        break;
                    }

                    // Copy
                    case KeyCode.C: {
                        clipboard = GetSelectedString();
                        return;
                    }
                }
            }

            switch (evt.keyCode) {
                /*
                    arrow key
                */
                case KeyCode.LeftArrow: {
                    MoveLeft(shift, ctrl);
                    return;
                }

                case KeyCode.RightArrow: {
                    MoveRight(shift, ctrl);
                    return;
                }

                case KeyCode.UpArrow: {
                    MoveUp(shift);
                    return;
                }

                case KeyCode.DownArrow: {
                    MoveDown(shift);
                    return;
                }
            }

            char c = evt.character;
           
            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3) c = '\n';

            if (c == 0) {
                if (Input.compositionString.Length > 0) {
                    UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e) {
            // キーハンドラっぽい
            KeyPressed(e);
        }

        /**
            選択範囲更新してると出る
        */
        public virtual void OnUpdateSelected(BaseEventData eventData) {
            if (!isFocused) return;

            bool consumedEvent = false;
            while (Event.PopEvent(m_ProcessingEvent)) {
                if (m_ProcessingEvent.rawType == EventType.KeyDown) {
                    consumedEvent = true;
                    KeyPressed(m_ProcessingEvent);
                }

                switch (m_ProcessingEvent.type) {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (m_ProcessingEvent.commandName) {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            if (consumedEvent) UpdateLabel();
            eventData.Use();
        }

        private string GetSelectedString() {
            if (!hasSelection) return string.Empty;

            int startPos = caretPositionInternal;
            int endPos = caretSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos) {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }
            var characters = m_VisibleContentsText.Substring(startPos, endPos - startPos);
            return characters;
        }

        private int FindtNextWordBegin() {
            if (caretSelectPositionInternal + 1 >= m_VisibleContentsText.Length) return m_VisibleContentsText.Length;

            int spaceLoc = m_VisibleContentsText.IndexOfAny(kSeparators, caretSelectPositionInternal + 1);

            if (spaceLoc == -1) spaceLoc = m_VisibleContentsText.Length;
            else spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl) {
            if (hasSelection && !shift) {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
                return;
            }

            int position;
            if (ctrl) position = FindtNextWordBegin();
            else position = caretSelectPositionInternal + 1;

            if (shift) caretSelectPositionInternal = position;
            else caretSelectPositionInternal = caretPositionInternal = position;
        }

        private int FindtPrevWordBegin() {
            if (caretSelectPositionInternal - 2 < 0) return 0;

            int spaceLoc = m_VisibleContentsText.LastIndexOfAny(kSeparators, caretSelectPositionInternal - 2);

            if (spaceLoc == -1) spaceLoc = 0;
            else spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl) {
            if (hasSelection && !shift) {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
                return;
            }

            int position;
            if (ctrl) position = FindtPrevWordBegin();
            else position = caretSelectPositionInternal - 1;

            if (shift) caretSelectPositionInternal = position;
            else caretSelectPositionInternal = caretPositionInternal = position;
        }

        private int DetermineCharacterLine(int charPos, TextGenerator generator) {
            for (int i = 0; i < generator.lineCount - 1; ++i) {
                if (generator.lines[i + 1].startCharIdx > charPos) return i;
            }

            return generator.lineCount - 1;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar) {
            if (originalPos > cachedInputTextGenerator.characterCountVisible) return 0;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the first line return first character
            if (originLine <= 0) return goToFirstChar ? 0 : originalPos;

            int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

            for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i) {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x) return i;
            }
            return endCharIdx;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineDownCharacterPosition(int originalPos, bool goToLastChar) {
            if (originalPos >= cachedInputTextGenerator.characterCountVisible) return m_VisibleContentsText.Length;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine + 1 >= cachedInputTextGenerator.lineCount) return goToLastChar ? m_VisibleContentsText.Length : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);

            for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i) {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x) return i;
            }
            return endCharIdx;
        }

        private void MoveDown(bool shift) {
            MoveDown(shift, true);
        }

        private void MoveDown(bool shift, bool goToLastChar) {
            if (hasSelection && !shift) {
                // If we have a selection and press down without shift,
                // set caret position to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar);
            
            if (shift) caretSelectPositionInternal = position;
            else caretPositionInternal = caretSelectPositionInternal = position;
        }

        private void MoveUp(bool shift) {
            MoveUp(shift, true);
        }

        private void MoveUp(bool shift, bool goToFirstChar) {
            if (hasSelection && !shift) {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar);

            if (shift) caretSelectPositionInternal = position;
            else caretSelectPositionInternal = caretPositionInternal = position;
        }

        /// <summary>
        /// Update the visual text Text.
        /// </summary>
        protected void UpdateLabel() {
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

            var processed = m_VisibleContentsText;

            // Determine what will actually fit into the given line
            Vector2 extents = m_TextComponent.rectTransform.rect.size;

            var settings = m_TextComponent.GetGenerationSettings(extents);
            settings.generateOutOfBounds = true;

            cachedInputTextGenerator.Populate(processed, settings);

            SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);

            // ここで、表示内容を表示してるんだけど、外部からいじれそうにない。うーーーーんん、、、、、
            processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

            SetCaretVisible();


            m_TextComponent.text = processed;

            MarkGeometryAsDirty();
            m_PreventFontCallback = false;
        }

        private bool IsSelectionVisible() {
            if (m_DrawStart > caretPositionInternal || m_DrawStart > caretSelectPositionInternal) return false;
            if (m_DrawEnd < caretPositionInternal || m_DrawEnd < caretSelectPositionInternal) return false;
            return true;
        }

        private static int GetLineStartPosition(TextGenerator gen, int line) {
            line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
            return gen.lines[line].startCharIdx;
        }

        private static int GetLineEndPosition(TextGenerator gen, int line) {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count) return gen.lines[line + 1].startCharIdx - 1;
            return gen.characterCountVisible;
        }

        private void SetDrawRangeToContainCaretPosition(int caretPos) {
            // We don't have any generated lines generation is not valid.
            if (cachedInputTextGenerator.lineCount <= 0) return;

            // the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
            // the values returned by the TextGenerator.lines[x].height for instance.
            Vector2 extents = cachedInputTextGenerator.rectExtents.size;
            
            var lines = cachedInputTextGenerator.lines;
            int caretLine = DetermineCharacterLine(caretPos, cachedInputTextGenerator);

            if (caretPos > m_DrawEnd) {
                // Caret comes after drawEnd, so we need to move drawEnd to the end of the line with the caret
                m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, caretLine);
                float bottomY = lines[caretLine].topY - lines[caretLine].height;
                int startLine = caretLine;
                while (startLine > 0) {
                    float topY = lines[startLine - 1].topY;
                    if (topY - bottomY > extents.y) break;
                    startLine--;
                }
                m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            } else {
                if (caretPos < m_DrawStart) {
                    // Caret comes before drawStart, so we need to move drawStart to an earlier line start that comes before caret.
                    m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, caretLine);
                }

                int startLine = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
                int endLine = startLine;

                float topY = lines[startLine].topY;
                float bottomY = lines[endLine].topY - lines[endLine].height;

                while (endLine < lines.Count - 1) {
                    bottomY = lines[endLine + 1].topY - lines[endLine + 1].height;
                    if (topY - bottomY > extents.y) break;
                    ++endLine;
                }

                m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);

                while (startLine > 0) {
                    topY = lines[startLine - 1].topY;
                    if (topY - bottomY > extents.y) break;
                    startLine--;
                }

                m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            }
        }

        private void MarkGeometryAsDirty() {
            #if UNITY_EDITOR
            {
                if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null) return;
            }
            #endif

            // 範囲描画他に関連している
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update) {
            switch (update) {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete() {
            // Debug.LogError("LayoutComplete");呼ばれてる
        }

        public virtual void GraphicUpdateComplete() {
            // Debug.LogError("GraphicUpdateComplete");呼ばれてる
        }

        /**
            主にキャレットとかハイライト周りの描画に関わっている。
        */
        private void UpdateGeometry() {
            #if UNITY_EDITOR
                if (!Application.isPlaying) return;
            #endif

            
            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (!shouldHideMobileInput) return;

            if (m_CachedInputRenderer == null && m_TextComponent != null) {
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

            if (m_CachedInputRenderer == null) return;

            OnFillVBO(DrawingMesh);
            m_CachedInputRenderer.SetMesh(DrawingMesh);
        }

        private void AssignPositioningIfNeeded() {
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

        /**
            ハイライトの描画だけ関連してる。
        */
        private void OnFillVBO(Mesh vbo) {
            using (var helper = new VertexHelper()) {
                if (!isFocused) {
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
                
                if (!hasSelection) GenerateCaret(helper, roundingOffset);
                else GenerateHightlight(helper, roundingOffset);

                helper.FillMesh(vbo);
            }
        }

        private Vector2 DisplayCharPos (int charPos) {
            var cursorChar = m_TextComponent.cachedTextGenerator.characters[charPos];
            return cursorChar.cursorPos;
        } 

        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset) {
            if (!m_CaretVisible) return;

            if (m_CursorVerts == null) {
                CreateCursorVerts();
            }

            float width = m_CaretWidth;
            int adjustedPos = Mathf.Max(0, caretPositionInternal - m_DrawStart);
            
            var gen = m_TextComponent.cachedTextGenerator;

            if (gen == null) return;
            if (gen.lineCount == 0) return;

            Vector2 startPosition = Vector2.zero;

            // Calculate startPosition
            if (adjustedPos < gen.characters.Count) startPosition.x = DisplayCharPos(adjustedPos).x;
            startPosition.x /= m_TextComponent.pixelsPerUnit;

            // TODO: Only clamp when Text uses horizontal word wrap.
            if (startPosition.x > m_TextComponent.rectTransform.rect.xMax) startPosition.x = m_TextComponent.rectTransform.rect.xMax;

            int characterLine = DetermineCharacterLine(adjustedPos, gen);
            startPosition.y = gen.lines[characterLine].topY / m_TextComponent.pixelsPerUnit;
            float height = gen.lines[characterLine].height / m_TextComponent.pixelsPerUnit;

            for (int i = 0; i < m_CursorVerts.Length; i++) m_CursorVerts[i].color = caretColor;

            m_CursorVerts[0].position = new Vector3(startPosition.x, startPosition.y - height, 0.0f);// 左下
            m_CursorVerts[1].position = new Vector3(startPosition.x + width, startPosition.y - height, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, startPosition.y, 0.0f);//右上
            m_CursorVerts[3].position = new Vector3(startPosition.x, startPosition.y, 0.0f);// 上端ぽい
            
            if (roundingOffset != Vector2.zero) {
                for (int i = 0; i < m_CursorVerts.Length; i++) {
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
                if (currentChar == lastCharInLineIndex || currentChar == endChar) {
                    UICharInfo startCharInfo = gen.characters[startChar];
                    UICharInfo endCharInfo = gen.characters[currentChar];
                    Vector2 startPosition = new Vector2(startCharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit, gen.lines[currentLineIndex].topY / m_TextComponent.pixelsPerUnit);
                    Vector2 endPosition = new Vector2((endCharInfo.cursorPos.x + endCharInfo.charWidth) / m_TextComponent.pixelsPerUnit, startPosition.y - gen.lines[currentLineIndex].height / m_TextComponent.pixelsPerUnit);

                    // Checking xMin as well due to text generator not setting position if char is not rendered.
                    if (endPosition.x > m_TextComponent.rectTransform.rect.xMax
                     || endPosition.x < m_TextComponent.rectTransform.rect.xMin) endPosition.x = m_TextComponent.rectTransform.rect.xMax;

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

        public void ActivateInputField() {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable()) return;

            if (isFocused) {
                if (m_Keyboard != null && !m_Keyboard.active) {
                    m_Keyboard.active = true;
                    m_Keyboard.text = m_VisibleContentsText;
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

                Debug.LogError("キーボードのセットとか。モバイルかな。");
                // m_Keyboard = TouchScreenKeyboard.Open(m_VisibleContentsText, keyboardType, false, true);

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

                // m_CaretPosition = m_CaretSelectPosition = 0;

                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData) {
            DeactivateInputField();
            base.OnDeselect(eventData);
        }

        protected override void DoStateTransition(SelectionState state, bool instant) {
            if (m_HasDoneFocusTransition) state = SelectionState.Highlighted;
            else if (state == SelectionState.Pressed) m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }
    }

    internal static class SetPropertyUtility {
        public static bool SetColor(ref Color currentValue, Color newValue) {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a) return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T> {
            if (currentValue.Equals(newValue)) return false;

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
