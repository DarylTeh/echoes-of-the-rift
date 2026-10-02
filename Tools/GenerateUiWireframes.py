#!/usr/bin/env python3
"""Generate editable, self-contained Echoes of the Rift UI wireframe SVGs."""

from html import escape
from pathlib import Path

OUT = Path(__file__).resolve().parents[1] / "Design" / "Wireframes"
W, H = 1280, 720
BG, PANEL, TILE = "#15141b", "#24222e", "#302e3a"
LINE, TEXT, MUTED, ACCENT = "#777386", "#f2eff8", "#aaa6b8", "#5cd3d0"


def box(x, y, w, h, label, kind="panel", primary=False, cols=0, rows=0):
    return {"x": x, "y": y, "w": w, "h": h, "label": label, "kind": kind,
            "primary": primary, "cols": cols, "rows": rows}


def P(x, y, w, h, label): return box(x, y, w, h, label)
def T(x, y, w, h, label): return box(x, y, w, h, label, "text")
def B(x, y, w, h, label, primary=False): return box(x, y, w, h, label, "button", primary)
def G(x, y, w, h, label, cols, rows): return box(x, y, w, h, label, "grid", cols=cols, rows=rows)
def I(x, y, w, h, label): return box(x, y, w, h, label, "image")


SCREENS = [
    ("01-sign-in", "Sign in", "EXISTING FLOW · REDESIGN PROPOSAL", "Choose the first action: Sign in or Create account?", [
        I(100, 160, 450, 360, "HERO / GAME LOGO"), T(100, 555, 450, 52, "ECHOES OF THE RIFT"),
        P(650, 115, 530, 500, "ACCOUNT"), T(700, 510, 430, 42, "Welcome back"),
        P(710, 405, 410, 58, "Username field"), P(710, 325, 410, 58, "Password field"),
        B(710, 225, 410, 64, "Sign in", True), T(710, 175, 195, 36, "Create account"), T(925, 175, 195, 36, "Forgot password?")]),
    ("02-create-account", "Create account", "EXISTING FLOW · REDESIGN PROPOSAL", "Should account creation follow sign-in, or be the default for new players?", [
        I(100, 170, 420, 340, "HERO / GAME LOGO"), P(610, 95, 570, 530, "CREATE ACCOUNT"),
        T(675, 530, 440, 42, "Create your account"), P(690, 425, 410, 58, "Username"),
        P(690, 345, 410, 58, "Password · 5+ characters"), P(690, 265, 410, 58, "Confirm password"),
        B(690, 165, 410, 62, "Create account", True), T(735, 115, 320, 30, "Already have an account? Sign in")]),
    ("03-account-recovery", "Account recovery", "EXISTING FLOW · REDESIGN PROPOSAL", "Should recovery use the saved recovery code, email, or both?", [
        P(330, 75, 620, 570, "RECOVER ACCOUNT"), T(410, 550, 460, 45, "Recover your account"),
        T(405, 495, 470, 48, "Use your recovery method to reset the password."),
        P(420, 400, 440, 58, "Username"), P(420, 320, 440, 58, "Recovery code"),
        P(420, 240, 440, 58, "New password"), B(420, 145, 440, 60, "Reset password", True),
        T(450, 95, 380, 30, "Back to sign in")]),
    ("04-server-unavailable", "Server unavailable", "EXISTING FLOW", "Should Quit remain here, or should Retry be the only action?", [
        I(100, 155, 450, 380, "TITLE SCREEN / DISABLED BACKDROP"), P(585, 170, 570, 380, "CONNECTION REQUIRED"),
        T(650, 440, 440, 55, "Can't connect right now"), T(650, 355, 440, 68, "Your account is safe. Check your connection and retry."),
        B(680, 250, 360, 62, "Retry connection", True), B(680, 180, 360, 52, "Quit" )]),
    ("05-character-creation", "Create your hero", "EXISTING FLOW · REDESIGN PROPOSAL", "Which choices should players make before their first run?", [
        P(75, 90, 640, 540, "HERO PREVIEW"), I(250, 210, 300, 330, "CHARACTER"),
        T(140, 565, 500, 34, "Wayfarer · cosmetic choices"), P(755, 90, 450, 540, "APPEARANCE"),
        T(800, 555, 360, 32, "Origin / race"), G(800, 455, 360, 78, "Choose a portrait", 5, 1),
        T(800, 405, 360, 30, "Skin tone"), G(800, 342, 360, 44, "Curated swatches", 8, 1),
        T(800, 292, 360, 30, "Hair"), G(800, 228, 360, 44, "Styles / colors", 6, 1),
        B(800, 135, 360, 62, "Continue", True)]),
    ("06-town-hub", "Rift Haven", "EXISTING FLOW · REDESIGN PROPOSAL", "Which three destinations belong in primary navigation?", [
        P(30, 565, 300, 105, "HERO PORTRAIT · NAME · LEVEL · HP"), P(935, 610, 300, 56, "GOLD · GEMS · PLUS"),
        P(34, 285, 180, 250, "ADVENTURE · GOALS · SOCIAL"), P(1070, 270, 175, 300, "BACKPACK · MAIL · EVENTS · SETTINGS"),
        P(245, 120, 790, 480, "TOWN WORLD / SCROLLING CAMERA"), I(555, 305, 150, 190, "PLAYER HERO"),
        T(475, 205, 330, 35, "Nearby: Hephaestus · Shop"), B(430, 78, 420, 62, "Campaign · Continue", True),
        T(440, 27, 400, 30, "Context action appears only near a real destination")]),
    ("07-mode-details", "Campaign details", "EXISTING FLOW · REDESIGN PROPOSAL", "Should mode details open inline or as a separate page?", [
        P(230, 65, 820, 590, "MODE OVERVIEW"), I(295, 315, 300, 230, "MODE ART / MAP"),
        T(635, 525, 330, 45, "CAMPAIGN"), T(635, 445, 330, 76, "Four boss-led rooms · solo or co-op test"),
        T(635, 350, 330, 55, "Reward preview · recommended power"),
        B(635, 215, 310, 62, "Enter campaign", True), B(635, 140, 310, 52, "Back to town")]),
    ("08-backpack", "Backpack", "EXISTING FLOW · REDESIGN PROPOSAL", "What should the first filter be: slot, rarity, or all items?", [
        P(28, 92, 78, 550, "HERO · BAG · SKILLS · SHOP"), P(120, 92, 420, 550, "HERO / EQUIPPED GEAR"),
        I(245, 300, 170, 210, "HERO PREVIEW"), T(175, 145, 310, 34, "Level · combat stats"),
        P(560, 92, 690, 550, "COLLECTION"), T(595, 590, 300, 30, "Items · owned count"),
        B(915, 575, 135, 42, "Filter"), B(1060, 575, 145, 42, "Sort"),
        G(590, 170, 620, 375, "ITEMS · RARITY BORDER · COUNT", 5, 4),
        B(785, 112, 205, 42, "Previous / Page / Next"), B(1150, 580, 58, 48, "X")]),
    ("09-item-details", "Item details", "EXISTING FLOW · REDESIGN PROPOSAL", "Which item stats and actions matter most to you?", [
        P(300, 58, 680, 604, "ITEM INSPECTION"), I(350, 455, 150, 150, "ITEM ART"),
        T(535, 565, 385, 45, "ITEM NAME · RARITY · ENHANCEMENT"),
        T(535, 505, 385, 45, "Weapon · class · base stats"),
        P(350, 275, 570, 175, "EQUIPPED VS NEW · STAT CHANGE · SKILL SYNERGY"),
        T(350, 205, 570, 42, "Description / lore / source"),
        B(350, 105, 260, 62, "Equip", True), B(635, 105, 160, 54, "Upgrade"), B(810, 105, 110, 54, "Close")]),
    ("10-skills-loadout", "Skills & loadout", "CONCEPT · DEDICATED PAGE NOT IMPLEMENTED", "How many skills should be equipped at once?", [
        P(32, 90, 450, 555, "EQUIPPED SKILLS"), T(78, 590, 345, 32, "ACTIVE LOADOUT"),
        G(75, 310, 365, 220, "SLOTS · ORDER · HOTKEY", 2, 2), T(75, 255, 370, 38, "Combat preview · stance / cooldown"),
        B(75, 130, 365, 58, "Save loadout", True), P(500, 90, 750, 555, "SKILL BOOK COLLECTION"),
        T(550, 590, 600, 32, "Owned · class · element · rarity"), G(550, 180, 650, 360, "BOOKS · LEVEL · STARS", 5, 3),
        B(1050, 110, 150, 48, "Filter")]),
    ("11-shop", "Shop", "EXISTING FLOW · REDESIGN PROPOSAL", "Which shop categories should be tabs versus a left rail?", [
        P(25, 565, 220, 110, "GOLD · GEMS"), P(270, 570, 935, 72, "BEST BUYS · D.SHOP · RESTOCK"),
        P(25, 105, 205, 440, "BARGAIN · WEAPONS · SKILLS · MATERIALS"),
        P(255, 105, 955, 440, "OFFERS"),
        P(285, 155, 275, 345, "OFFER CARD 1"), I(360, 275, 120, 120, "ITEM ART"), T(310, 210, 225, 38, "Name · stock · price"), B(310, 165, 225, 45, "Inspect"),
        P(590, 155, 275, 345, "OFFER CARD 2"), I(665, 275, 120, 120, "ITEM ART"), T(615, 210, 225, 38, "Name · stock · price"), B(615, 165, 225, 45, "Inspect"),
        P(895, 155, 275, 345, "OFFER CARD 3"), I(970, 275, 120, 120, "ITEM ART"), T(920, 210, 225, 38, "Name · stock · price"), B(920, 165, 225, 45, "Inspect")]),
    ("12-settings-controls", "Settings · Controls", "EXISTING FLOW", "Which controls should be customizable on PC and phone?", [
        P(220, 65, 840, 590, "SETTINGS"), P(250, 115, 190, 480, "CONTROLS · ACCOUNTS · AUDIO · DISPLAY"),
        T(485, 555, 500, 40, "CONTROLS"), P(485, 420, 500, 95, "Move · WASD / left stick · Change"),
        P(485, 305, 500, 95, "Attack · mouse / right stick · Change"),
        P(485, 190, 500, 95, "Skills · Q / E / R · Change"), B(815, 100, 170, 50, "Close", True)]),
    ("13-settings-account", "Settings · Account", "EXISTING FLOW", "Should account status show linked devices or recovery readiness?", [
        P(220, 65, 840, 590, "SETTINGS"), P(250, 115, 190, 480, "CONTROLS · ACCOUNTS · AUDIO · DISPLAY"),
        T(485, 555, 500, 40, "ACCOUNT"), P(485, 380, 500, 130, "Username · linked status · save status"),
        T(505, 330, 450, 38, "Recovery code · view / replace safely"),
        B(505, 230, 230, 55, "Manage recovery"), B(755, 100, 230, 52, "Close", True),
        T(505, 170, 440, 32, "Sign out stays here and requires confirmation")]),
    ("14-sign-out-confirm", "Sign out?", "EXISTING FLOW · SAFETY CHECK", "Is this confirmation clear enough without making sign-out too prominent?", [
        P(350, 170, 580, 380, "CONFIRMATION"), T(405, 440, 470, 48, "Sign out of this device?"),
        T(405, 360, 470, 68, "Your saved progress remains on your account."),
        B(415, 250, 225, 60, "Stay in game", True), B(660, 250, 225, 60, "Sign out")]),
    ("15-events-list", "Events", "EXISTING FLOW · REDESIGN PROPOSAL", "Which event information should be visible before opening details?", [
        P(135, 70, 1010, 580, "EVENTS"), T(195, 590, 600, 35, "EVENTS · SERVER TIME"),
        P(180, 465, 920, 95, "FEATURED EVENT · ART · REWARD · TIME LEFT"), B(890, 485, 165, 54, "Go", True),
        P(185, 350, 910, 78, "Event row · goal progress · reward preview · ends in"),
        P(185, 255, 910, 78, "Event row · goal progress · reward preview · ends in"),
        P(185, 160, 910, 78, "Event row · goal progress · reward preview · ends in"),
        B(980, 95, 120, 46, "Close")]),
    ("16-event-details", "Event details", "EXISTING FLOW · REDESIGN PROPOSAL", "Should Go open a mode, or should event progress happen anywhere?", [
        P(150, 65, 980, 590, "EVENT DETAIL"), I(205, 370, 300, 220, "EVENT ART"),
        T(555, 550, 500, 48, "EVENT TITLE · TIME LEFT"), T(555, 465, 500, 62, "Goal · eligibility · event rules"),
        P(555, 300, 500, 105, "REWARD TRACK / CLAIM STATUS"), P(205, 150, 850, 92, "PERSONAL PROGRESS · NEXT MILESTONE"),
        B(745, 90, 250, 56, "Go to activity", True), B(470, 90, 230, 52, "Back to events")]),
    ("17-reward-inbox", "Reward inbox", "EXISTING FLOW · REDESIGN PROPOSAL", "Do you want a reward reveal, a quick claim, or both?", [
        P(155, 60, 970, 600, "REWARD INBOX"), T(210, 595, 560, 36, "Unclaimed rewards · newest first"),
        P(200, 470, 865, 82, "EVENT / SOURCE · REWARD ART · AMOUNT · DATE"), B(900, 486, 140, 52, "Collect", True),
        P(200, 365, 865, 82, "EVENT / SOURCE · REWARD ART · AMOUNT · DATE"), B(900, 381, 140, 52, "Collect", True),
        P(200, 260, 865, 82, "EVENT / SOURCE · REWARD ART · AMOUNT · DATE"), B(900, 276, 140, 52, "Collect", True),
        B(875, 95, 165, 50, "Refresh"), B(1050, 95, 65, 50, "X")]),
    ("18-combat-solo", "Campaign combat", "EXISTING FLOW · REDESIGN PROPOSAL", "Which combat controls should stay visible all the time?", [
        P(18, 575, 300, 120, "PORTRAIT · NAME · LEVEL · HP / MP"), P(960, 622, 295, 54, "GOLD · GEMS · PLUS"),
        P(420, 625, 440, 54, "BOSS NAME · HP BAR · PHASE"), P(25, 370, 260, 170, "OBJECTIVE · KILL / TIMER"),
        P(1025, 395, 210, 190, "MINIMAP / ROOM"), P(25, 60, 155, 155, "MOVE STICK"),
        I(480, 240, 220, 275, "PLAYER + COMBAT SPACE"), B(1060, 75, 150, 150, "ATTACK", True),
        G(905, 80, 145, 240, "3 SKILLS", 1, 3), B(1160, 285, 66, 66, "BAG"),
        B(1160, 205, 66, 66, "DODGE"), B(1160, 125, 66, 66, "STANCE")]),
    ("19-combat-coop", "Co-op combat", "EXISTING FLOW · REDESIGN PROPOSAL", "Should party health be top-left or beside the minimap?", [
        P(18, 575, 300, 120, "YOU · PORTRAIT · HP / MP"), P(18, 440, 260, 100, "ALLY · PORTRAIT · HP · DOWNED STATE"),
        P(960, 622, 295, 54, "GOLD · GEMS"), P(420, 625, 440, 54, "BOSS NAME · HEALTH · PHASE"),
        P(1025, 395, 210, 190, "MINIMAP · YOU · ALLY · BOSS"), P(25, 60, 155, 155, "MOVE STICK"),
        I(480, 240, 220, 275, "PLAYER + ALLY + COMBAT SPACE"), B(1060, 75, 150, 150, "ATTACK", True),
        G(905, 80, 145, 240, "3 SKILLS", 1, 3), B(1160, 285, 66, 66, "BAG"),
        B(1160, 205, 66, 66, "DODGE"), B(1160, 125, 66, 66, "STANCE")]),
    ("20-pause-menu", "Menu", "EXISTING FLOW · REDESIGN PROPOSAL", "Should players be allowed to leave a run from pause?", [
        P(390, 95, 500, 530, "MENU"), T(470, 545, 340, 52, "IN RUN"), B(460, 415, 360, 65, "Resume", True),
        B(460, 325, 360, 54, "Settings"), B(460, 245, 360, 54, "Restart run"), B(460, 165, 360, 54, "Return to town")]),
    ("21-room-clear", "Room cleared", "EXISTING FLOW · REDESIGN PROPOSAL", "What reward details make a room clear feel satisfying?", [
        P(300, 75, 680, 570, "ROOM CLEAR"), I(390, 380, 150, 155, "BOSS / ROOM ART"),
        T(575, 500, 340, 55, "ROOM CLEARED"), T(575, 430, 340, 42, "Boss name · clear time"),
        P(390, 245, 520, 105, "REWARD ITEM · GOLD · FIRST-CLEAR BONUS"),
        B(520, 115, 300, 68, "Next room", True), B(390, 115, 115, 54, "Details")]),
    ("22-campaign-complete", "Campaign complete", "EXISTING FLOW · REDESIGN PROPOSAL", "What should completion celebrate beyond loot?", [
        P(270, 55, 740, 610, "CAMPAIGN COMPLETE"), I(360, 390, 170, 185, "FINAL BOSS / HERO"),
        T(580, 520, 370, 55, "VICTORY"), T(580, 445, 370, 40, "Campaign · difficulty · time"),
        P(365, 265, 555, 105, "REWARDS · UNLOCKS · PERSONAL BEST"),
        B(535, 115, 355, 66, "Return to Rift Haven", True)]),
    ("23-defeat", "Run ended", "EXISTING FLOW · REDESIGN PROPOSAL", "What should the player be able to do after a defeat?", [
        P(310, 75, 660, 570, "DEFEAT"), I(390, 370, 150, 165, "HERO / BOSS ART"),
        T(570, 495, 330, 55, "RUN ENDED"), T(570, 420, 330, 55, "Room reached · progress saved"),
        P(390, 245, 500, 100, "RECOVERABLE PROGRESS / UNCLAIMED LOOT"),
        B(510, 125, 310, 66, "Return to town", True)]),
    ("24-reward-save-error", "Reward save needs attention", "EXISTING FLOW · REDESIGN PROPOSAL", "Should the game remain here until the reward is safely saved?", [
        P(295, 95, 690, 530, "REWARD RECOVERY"), T(365, 530, 550, 52, "Reward not saved yet"),
        T(365, 440, 550, 72, "Your run reward is being kept for retry. Do not close the game."),
        P(365, 320, 550, 75, "ITEM · CURRENCY · SAVE STATUS"),
        B(485, 190, 310, 64, "Retry save", True), T(460, 130, 350, 30, "Connection / storage help")]),
    ("25-connection-lost", "Connection lost", "EXISTING FLOW · REDESIGN PROPOSAL", "Should reconnect happen automatically with a visible countdown?", [
        P(305, 135, 670, 450, "CONNECTION"), T(380, 480, 520, 55, "Connection interrupted"),
        T(385, 395, 510, 70, "Reconnecting… Your account and saved progress are safe."),
        P(410, 300, 460, 65, "Connection status · attempt / elapsed"),
        B(470, 190, 340, 64, "Reconnect", True)]),
    ("26-player-downed", "Hero down", "EXISTING FLOW · REDESIGN PROPOSAL", "Should self-revive and teammate rescue have different affordances?", [
        I(455, 350, 160, 200, "DOWNED HERO"), P(340, 105, 600, 235, "ALLY RESCUE / REVIVE"),
        T(400, 280, 480, 44, "You are down"), T(400, 225, 480, 44, "An ally can revive you nearby"),
        B(435, 145, 190, 58, "Self revive"), B(650, 145, 190, 58, "Wait for ally", True)]),
    ("27-leaderboard", "Wayfarer rankings", "EXISTING FLOW · REDESIGN PROPOSAL", "Which ranking should be the default: campaign, power, or season?", [
        P(110, 70, 1060, 585, "RANKINGS"), T(160, 595, 580, 35, "LEADERBOARD · SEASON / MODE FILTER"),
        G(155, 175, 590, 375, "RANK · PLAYER · SCORE · VIEW", 1, 8),
        P(780, 165, 330, 390, "SELECTED PLAYER"), I(860, 365, 170, 170, "HERO PREVIEW"),
        T(820, 305, 250, 55, "Name · build · score"), B(885, 100, 150, 50, "Close")]),
    ("28-daily-goals", "Goals & quests", "CONCEPT · PAGE NOT IMPLEMENTED", "Which goals should be daily, weekly, and campaign-long?", [
        P(120, 60, 1040, 600, "GOALS"), P(150, 100, 180, 510, "ADVENTURE · DIARY · GOALS"),
        T(375, 595, 610, 35, "TODAY'S GOALS · RESET TIMER"), P(375, 490, 710, 75, "GOAL · PROGRESS BAR · REWARD · GO"),
        P(375, 395, 710, 75, "GOAL · PROGRESS BAR · REWARD · GO"),
        P(375, 300, 710, 75, "GOAL · PROGRESS BAR · REWARD · GO"),
        P(375, 175, 710, 85, "MILESTONE CHESTS · COMPLETION TRACK"), B(960, 100, 125, 50, "Claim", True)]),
    ("29-player-profile", "Player profile", "CONCEPT · PAGE NOT IMPLEMENTED", "What should another player learn from your profile?", [
        P(110, 65, 1060, 590, "PROFILE"), P(140, 110, 190, 500, "MY INFO · ARCHIVE"),
        P(365, 115, 470, 490, "AVATAR / FRAME · OWNED / LOCKED"), I(510, 335, 180, 200, "HERO PREVIEW"),
        B(430, 165, 160, 52, "Avatar"), B(610, 165, 160, 52, "Frame"),
        P(865, 115, 270, 490, "NAME · LEVEL · GUILD · SHOWCASE"), B(890, 130, 215, 54, "Apply selection", True)]),
    ("30-matchmaking", "Co-op party", "CONCEPT · CURRENTLY ONLY A TEST GATE", "How many players should a party support at launch?", [
        P(180, 60, 920, 600, "PARTY READY"), T(255, 585, 770, 36, "CAMPAIGN CO-OP · REGION · PRIVACY"),
        G(255, 290, 770, 230, "PLAYER SLOTS · READY STATUS · INVITE", 2, 2),
        P(255, 190, 770, 65, "DIFFICULTY · RECOMMENDED POWER · REWARDS"),
        B(560, 95, 300, 64, "Start expedition", True), B(270, 100, 145, 52, "Leave party")]),
]


def draw_node(n):
    x, y, w, h = n["x"], n["y"], n["w"], n["h"]
    label, kind = escape(n["label"]), n["kind"]
    if kind == "text":
        return f'<text x="{x}" y="{H-y}" text-anchor="start" class="body">{label}</text>'
    fill = ACCENT if n["primary"] else (TILE if kind in ("image", "grid") else PANEL)
    stroke = ACCENT if n["primary"] else LINE
    out = [f'<rect x="{x}" y="{H-y-h}" width="{w}" height="{h}" rx="12" fill="{fill}" fill-opacity=".94" stroke="{stroke}" stroke-width="2"/>']
    if kind == "image":
        out.append(f'<path d="M{x+12} {H-y-h-12} L{x+w-12} {H-y-12} M{x+w-12} {H-y-h-12} L{x+12} {H-y-12}" stroke="{LINE}" stroke-width="1"/>')
    if kind == "grid":
        cols, rows = max(1, n["cols"]), max(1, n["rows"])
        gap, pad, header = 8, 8, 28
        cw = (w - 2*pad - gap*(cols-1))/cols
        ch = (h - header - 2*pad - gap*(rows-1))/rows
        for r in range(rows):
            for c in range(cols):
                xx, yy = x+pad+c*(cw+gap), H-y-h+header+pad+r*(ch+gap)
                out.append(f'<rect x="{xx:.1f}" y="{yy:.1f}" width="{cw:.1f}" height="{ch:.1f}" rx="7" fill="{BG}" stroke="{LINE}" stroke-width="1"/>')
        out.append(f'<text x="{x+w/2}" y="{H-y-h+20}" text-anchor="middle" class="small">{label}</text>')
    else:
        cls = "button" if kind == "button" else "small" if kind == "image" else "paneltitle"
        out.append(f'<text x="{x+w/2}" y="{H-y-h/2+5}" text-anchor="middle" class="{cls}">{label}</text>')
    return "".join(out)


def render(slug, title, status, question, nodes):
    contents = "".join(draw_node(n) for n in nodes)
    return f'''<svg xmlns="http://www.w3.org/2000/svg" xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" width="{W}" height="{H}" viewBox="0 0 {W} {H}">
<title>{escape(title)} — Echoes of the Rift wireframe</title>
<desc>Editable low-fidelity screen layout. All boxes, labels and controls are separate vector elements.</desc>
<rect width="{W}" height="{H}" fill="{BG}"/>
<text x="32" y="38" class="heading">{escape(title)}</text>
<text x="1245" y="36" text-anchor="end" class="tag">{escape(status)}</text>
<line x1="32" y1="54" x2="1248" y2="54" stroke="{LINE}" stroke-width="1"/>
{contents}
<line x1="32" y1="676" x2="1248" y2="676" stroke="{LINE}" stroke-width="1"/>
<text x="34" y="704" class="small">DESIGN QUESTION</text><text x="190" y="704" class="body">{escape(question)}</text>
<style>
  text {{ font-family: Arial, sans-serif; fill: {TEXT}; }}
  .heading {{ font-size: 25px; font-weight: 700; }} .tag {{ font-size: 12px; fill: {ACCENT}; font-weight: 700; letter-spacing: 1px; }}
  .paneltitle {{ font-size: 15px; font-weight: 600; fill: {MUTED}; letter-spacing: .6px; }}
  .body {{ font-size: 17px; fill: {TEXT}; }} .small {{ font-size: 12px; fill: {MUTED}; }}
  .button {{ font-size: 17px; font-weight: 700; fill: {BG}; }}
</style>
</svg>'''


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    cards = []
    for slug, title, status, question, nodes in SCREENS:
        path = OUT / f"{slug}.svg"
        path.write_text(render(slug, title, status, question, nodes), encoding="utf-8")
        cards.append(f'''<a class="card" href="{path.name}"><img src="{path.name}" alt="{escape(title)} wireframe"><span>{escape(title)}</span><small>{escape(status)}</small></a>''')
    index = f'''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Echoes UI wireframes</title>
<style>body{{margin:0;padding:28px;background:#15141b;color:#f2eff8;font:15px Arial,sans-serif}}h1{{margin:0 0 8px}}p{{color:#aaa6b8}}main{{display:grid;grid-template-columns:repeat(auto-fit,minmax(330px,1fr));gap:18px}}.card{{color:inherit;text-decoration:none;background:#24222e;border:1px solid #777386;border-radius:12px;padding:10px;display:flex;flex-direction:column;gap:8px}}img{{width:100%;height:auto;border-radius:6px}}span{{font-weight:700}}small{{color:#5cd3d0}}</style>
<h1>Echoes of the Rift · UI wireframes</h1><p>{len(SCREENS)} independently editable SVG screens. Click a preview to open the SVG. Open it in Figma, Illustrator or Inkscape to move labels and shapes.</p><main>{''.join(cards)}</main></html>'''
    (OUT / "index.html").write_text(index, encoding="utf-8")
    readme = f'''# Echoes of the Rift UI wireframes

This is a first-pass, editable layout pack with **{len(SCREENS)} separate SVG files**. It is intentionally low fidelity: boxes and labels define hierarchy and interaction, not final art, colors, typography or exact copy. Each screen is designed on a 1280×720 landscape canvas and includes one design question at the bottom for your feedback.

Open `index.html` for a visual index. Each SVG is a standalone vector document that can be edited in Figma, Illustrator, Inkscape or a text editor. The generator is `Tools/GenerateUiWireframes.py`.

Screens tagged **EXISTING FLOW** represent current player-facing pages or interactions. **CONCEPT** pages identify gaps found in the strict review; they are not implemented game screens. A proposal label means the layout is a suggestion to edit, not a locked decision.

## Give feedback

Edit the SVG directly, or send the filename and changes such as “make the hero larger,” “move the quest rail to the right,” or “remove these two controls.” The footer question on each page is a prompt for the decisions that would most affect implementation. You can return a handful of edited SVGs first; we can revise the rest to match.

## Screen files

''' + "\n".join(f"- `{slug}.svg` — {title} ({status})" for slug, title, status, _, _ in SCREENS) + "\n"
    (OUT / "README.md").write_text(readme, encoding="utf-8")
    print(f"Generated {len(SCREENS)} editable SVG screens and an HTML index.")


if __name__ == "__main__":
    main()
