using BlazorStudio.ClassLib.Keyboard;

namespace BlazorStudio.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    private partial class StateMachine
    {
        public static PlainTextEditorRecord HandleMetaKey(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            switch (keyDownEventRecord.Key)
            {
                case KeyboardKeyFacts.MetaKeys.BACKSPACE_KEY:
                    return HandleBackspaceKey(focusedPlainTextEditorRecord, keyDownEventRecord);
                default:
                   return focusedPlainTextEditorRecord;
            }
        }

        public static PlainTextEditorRecord HandleBackspaceKey(PlainTextEditorRecord focusedPlainTextEditorRecord,
            KeyDownEventRecord keyDownEventRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.Default)
            {
                // Remove character from word

                return HandleDefaultBackspace(focusedPlainTextEditorRecord, keyDownEventRecord);
            }

            if (focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.StartOfRow &&
                focusedPlainTextEditorRecord.GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>().List.Count > 1)
            {
                // Remove newline character

                if (!keyDownEventRecord.IsForced)
                {
                    var previousRowIndex = focusedPlainTextEditorRecord.CurrentRowIndex - 1;
                    var previousRow = focusedPlainTextEditorRecord.List[previousRowIndex];

                    var characterIndexTotal = 0;

                    foreach (var token in previousRow.List)
                    {
                        characterIndexTotal += token.CopyText.Length;
                    }

                    focusedPlainTextEditorRecord.FileHandle.Edit
                        .Remove(previousRowIndex,
                            characterIndexTotal - 1,
                            characterCount: 1);
                }

                focusedPlainTextEditorRecord = MoveCurrentRowToEndOfPreviousRow(focusedPlainTextEditorRecord);
            }
            else
            {
                // Remove non word token (perhaps whitespace)

                if (!keyDownEventRecord.IsForced)
                {
                    if (focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.StartOfRow)
                    {
                        var previousRowIndex = focusedPlainTextEditorRecord.CurrentRowIndex - 1;
                        var previousRow = focusedPlainTextEditorRecord.List[previousRowIndex];

                        var characterIndexTotal = 0;

                        foreach (var token in previousRow.List)
                        {
                            characterIndexTotal += token.CopyText.Length;
                        }

                        focusedPlainTextEditorRecord.FileHandle.Edit
                            .Remove(previousRowIndex,
                                characterIndexTotal - 1,
                                characterCount: 1);
                    }
                    else
                    {
                        var characterIndex = CalculateCurrentTokenStartingCharacterIndexRespectiveToRow(focusedPlainTextEditorRecord)
                                             + focusedPlainTextEditorRecord.CurrentTextToken.IndexInPlainText.Value;

                        focusedPlainTextEditorRecord.FileHandle.Edit
                            .Remove(focusedPlainTextEditorRecord.CurrentRowIndex,
                                characterIndex - 1,
                                characterCount: 1);
                    }
                }

                focusedPlainTextEditorRecord = RemoveCurrentToken(focusedPlainTextEditorRecord);
            }

            focusedPlainTextEditorRecord = MergeTokensIfApplicable(focusedPlainTextEditorRecord);

            return focusedPlainTextEditorRecord;
        }
    }
}
