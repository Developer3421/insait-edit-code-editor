using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Insait_Edit_C_Sharp.Services;
using System;

namespace Insait_Edit_C_Sharp;

/// <summary>Window displaying the Insait Edit Code Editor User Agreement / EULA.</summary>
public partial class UserAgreementWindow : Window
{
    private const string DefaultEulaText = """
        INSAIT EDIT CODE EDITOR — END USER LICENSE AGREEMENT (EULA)
        ═════════════════════════════════════════════════════════════

        Last updated: August 2026

        Please read this End User License Agreement ("Agreement") carefully before
        using Insait Edit Code Editor ("the Software"). By installing or using the
        Software you agree to be bound by the terms below. If you do not agree to
        these terms, do not install or use the Software.

        1. LICENSE GRANT
        ─────────────────
        Subject to the terms of this Agreement, you are granted a limited,
        non-exclusive, non-transferable license to install and use the Software on
        devices you own or control for your personal development, internal business,
        or professional software development purposes, including the creation of
        both open-source and commercial projects.

        2. RESTRICTIONS
        ─────────────────
        The Software is free to use. You may use it to write, edit, build, run,
        publish, and package software free of charge, including commercial projects.

        You may not:
          • Create applications that copy or closely replicate the visual style,
            graphical design, or user interface appearance of Insait Edit Code Editor.
          • Reverse-engineer, decompile, or disassemble the Software except to the
            extent expressly permitted by applicable law.
          • Remove, obscure, or alter any proprietary notices, labels, or branding
            on the Software.

        3. SOURCE CONTROL AND GITHUB SERVICES
        ───────────────────────────────────────
        The Software provides optional integration with Git and GitHub services,
        including account sign-in, repository cloning, commit, push and pull
        operations, and GitHub Copilot-powered assistance. These features require
        your own credentials and are provided under GitHub's own terms.

        You acknowledge that:
        • commands you run may invoke external tools on your machine;
        • GitHub Copilot assistance and other cloud features may transmit code
          snippets and related data to the corresponding third-party service;
        • you are responsible for the content you publish or share through these
          services.

        The Software does not sell your data and does not monetize your source code.

        4. INTELLECTUAL PROPERTY
        ─────────────────────────
        All title, ownership, and intellectual property rights in and to the
        Software — including its custom Insait Code Editor, its styles, its icons,
        and its visual assets — remain with the Insait Edit Code Editor authors.
        The Software is protected by copyright laws and international treaty
        provisions. Code you write with the Software belongs to you.

        5. THIRD-PARTY COMPONENTS
        ──────────────────────────
        The Software may include open-source components governed by their own
        licenses (e.g. AvaloniaUI — MIT License, LiteDB). Those components are provided
        under the terms of their respective licenses.

        6. PRIVACY AND LOCAL STORAGE
        ─────────────────────────────
        Settings, recent projects, and account data are stored locally on your
        device and can be removed by uninstalling the Software. The Software does
        not display advertising and does not sell your personal information.

        7. DISCLAIMER OF WARRANTIES
        ────────────────────────────
        THE SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT.

        8. LIMITATION OF LIABILITY
        ───────────────────────────
        IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY INDIRECT, INCIDENTAL,
        SPECIAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF OR RELATING TO THIS
        AGREEMENT OR THE USE OF THE SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
        OF SUCH DAMAGES.

        9. TERMINATION
        ─────────────────
        This license is effective until you fail to comply with any term of this
        Agreement. Upon termination you must stop using and destroy all copies of
        the Software in your possession.

        10. GOVERNING LAW
        ──────────────────
        This Agreement shall be governed by and construed in accordance with
        applicable law, without regard to its conflict-of-law provisions.


        ═════════════════════════════════════════════════════════════
        Thank you for using Insait Edit Code Editor!
        """;

    public UserAgreementWindow()
    {
        InitializeComponent();
        ApplyLocalization();
        LocalizationService.LanguageChanged += (_, _) =>
            Avalonia.Threading.Dispatcher.UIThread.Post(ApplyLocalization);
    }

    private void ApplyLocalization()
    {
        var localize = (Func<string, string>)LocalizationService.Get;

        var title = this.FindControl<TextBlock>("TitleText");
        if (title != null) title.Text = localize("Tooltip.UserAgreement");
        Title = localize("Tooltip.UserAgreement");

        var footer = this.FindControl<TextBlock>("FooterText");
        if (footer != null) footer.Text = localize("UserAgreement.Footer");

        var closeText = this.FindControl<TextBlock>("CloseBtnText");
        if (closeText != null) closeText.Text = localize("UserAgreement.Close");

        var acceptText = this.FindControl<TextBlock>("AcceptBtnText");
        if (acceptText != null) acceptText.Text = localize("UserAgreement.Accept");

        var agreementBlock = this.FindControl<SelectableTextBlock>("AgreementText");
        if (agreementBlock != null)
        {
            var agreementText = localize("UserAgreement.Text");
            agreementBlock.Text = agreementText == "UserAgreement.Text"
                ? DefaultEulaText
                : agreementText;
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void Close_Click(object? sender, RoutedEventArgs e) => Close();

    private void Accept_Click(object? sender, RoutedEventArgs e) => Close();
}

